using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;

namespace ManufacturingOptimization.Common.Messaging;

public class RabbitMqService : IMessagePublisher, IMessageSubscriber, IMessagingInfrastructure, IDisposable
{
    private readonly ILogger<RabbitMqService> _logger;
    private readonly RabbitMqSettings _settings;

    private readonly Dictionary<string, EventingBasicConsumer> _consumers = new();
    private readonly Dictionary<string, string> _consumerTags = new(); // queueName -> consumerTag
    private readonly Dictionary<string, List<MessageHandler>> _handlers = new(); // Multiple handlers per queue
    private readonly ConcurrentDictionary<string, TaskCompletionSource<IMessage>> _pendingRequests = new();

    private IConnection _connection = null!;
    private IModel _channel = null!;
    private string _replyQueueName = null!;

    public RabbitMqService(
        IOptions<RabbitMqSettings> settings,
        ILogger<RabbitMqService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        InitializeRabbitMq();
    }

    private void InitializeRabbitMq()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(
                exchange: Exchanges.Optimization,
                type: ExchangeType.Topic,
                durable: true);

            _channel.ExchangeDeclare(
                exchange: Exchanges.System,
                type: ExchangeType.Topic,
                durable: true);

            _channel.ExchangeDeclare(
                exchange: Exchanges.Provider,
                type: ExchangeType.Topic,
                durable: true);

            _channel.ExchangeDeclare(
                exchange: Exchanges.Process,
                type: ExchangeType.Topic,
                durable: true);

            // Setup reply queue for RPC pattern
            _replyQueueName = _channel.QueueDeclare().QueueName;
            SetupReplyQueueConsumer();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not connect to RabbitMQ");
            throw;
        }
    }

    public void Publish<T>(string exchangeName, string routingKey, T message) where T : IMessage
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.Headers = new Dictionary<string, object>
        {
            ["MessageType"] = typeof(T).FullName ?? typeof(T).Name
        };

        _channel.BasicPublish(
            exchange: exchangeName,
            routingKey: routingKey,
            basicProperties: properties,
            body: body);
    }

    public async Task<TResponse?> RequestReplyAsync<TResponse>(string exchangeName, string routingKey, IMessage request, TimeSpan? timeout = null) where TResponse : class, IMessage
    {
        timeout ??= TimeSpan.FromSeconds(30);
        
        // Get correlation ID from ICommand interface
        string correlationId;
        if (request is ICommand command)
        {
            correlationId = command.CommandId.ToString();
        }
        else
        {
            correlationId = Guid.NewGuid().ToString();
        }
        
        var tcs = new TaskCompletionSource<IMessage>();
        _pendingRequests[correlationId] = tcs;

        try
        {
            // Set ReplyTo if this is a request-reply command
            if (request is IRequestReplyCommand replyCommand)
            {
                replyCommand.ReplyTo = _replyQueueName;
            }
            
            var json = JsonSerializer.Serialize(request, request.GetType());
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel.CreateBasicProperties();
            properties.CorrelationId = correlationId;
            properties.ReplyTo = _replyQueueName;
            properties.Headers = new Dictionary<string, object>
            {
                ["MessageType"] = request.GetType().FullName ?? request.GetType().Name
            };

            _channel.BasicPublish(
                exchange: exchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            // Wait for response with timeout
            using var cts = new CancellationTokenSource(timeout.Value);
            var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeout.Value, cts.Token));
            
            if (completedTask == tcs.Task)
            {
                var response = await tcs.Task;
                return response as TResponse;
            }
            else
            {
                return null;
            }
        }
        finally
        {
            _pendingRequests.TryRemove(correlationId, out _);
        }
    }

    private void SetupReplyQueueConsumer()
    {
        var consumer = new EventingBasicConsumer(_channel);
        
        consumer.Received += (sender, e) =>
        {
            try
            {
                var correlationId = e.BasicProperties.CorrelationId;
                
                if (_pendingRequests.TryGetValue(correlationId, out var tcs))
                {
                    var body = e.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    
                    string? messageType = null;
                    if (e.BasicProperties?.Headers?.TryGetValue("MessageType", out var typeObj) == true)
                    {
                        messageType = Encoding.UTF8.GetString((byte[])typeObj);
                    }

                    if (messageType != null)
                    {
                        var type = Type.GetType(messageType);
                        if (type != null)
                        {
                            var message = JsonSerializer.Deserialize(json, type) as IMessage;
                            if (message != null)
                            {
                                tcs.SetResult(message);
                            }
                        }
                    }
                }
                
                _channel.BasicAck(e.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing RPC reply");
                _channel.BasicNack(e.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(
            queue: _replyQueueName,
            autoAck: false,
            consumer: consumer);
    }

    public void PublishReply<TRequest, TResponse>(TRequest request, TResponse response) 
        where TRequest : IRequestReplyCommand 
        where TResponse : IMessage
    {
        // Automatically set CorrelationId if response is an event
        if (response is IEvent evt)
        {
            evt.CorrelationId = request.CommandId;
        }
        
        var json = JsonSerializer.Serialize(response);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.CorrelationId = request.CommandId.ToString();
        properties.Headers = new Dictionary<string, object>
        {
            ["MessageType"] = typeof(TResponse).FullName ?? typeof(TResponse).Name
        };

        _channel.BasicPublish(
            exchange: string.Empty, // Direct to queue
            routingKey: request.ReplyTo,
            basicProperties: properties,
            body: body);
    }

    public void Subscribe<T>(string queueName, Action<T> handler) where T : IMessage
    {
        // Store handler for this message type
        if (!_handlers.ContainsKey(queueName))
        {
            _handlers[queueName] = new List<MessageHandler>();
        }
        
        _handlers[queueName].Add(new MessageHandler 
        { 
            MessageType = typeof(T),
            Handler = handler 
        });

        // Create consumer only once per queue
        if (_consumers.ContainsKey(queueName))
            return;

        var consumer = new EventingBasicConsumer(_channel);
        _consumers[queueName] = consumer;

        consumer.Received += (sender, e) =>
        {
            try
            {
                var body = e.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                string? messageType = null;
                if (e.BasicProperties?.Headers?.TryGetValue("MessageType", out var typeObj) == true)
                {
                    messageType = Encoding.UTF8.GetString((byte[])typeObj);
                }

                // Try to invoke all matching handlers
                bool handled = false;
                foreach (var handlerInfo in _handlers[queueName])
                {
                    var expectedTypeName = handlerInfo.MessageType.FullName ?? handlerInfo.MessageType.Name;
                    
                    if (messageType == null || messageType == expectedTypeName)
                    {
                        try
                        {
                            var message = JsonSerializer.Deserialize(json, handlerInfo.MessageType);
                            if (message != null)
                            {
                                handlerInfo.Handler.DynamicInvoke(message);
                                handled = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error invoking handler for {MessageType}", expectedTypeName);
                        }
                    }
                }

                if (!handled)
                {
                    _logger.LogDebug("No handler found for message type {MessageType} on queue {Queue}", 
                        messageType, queueName);
                }

                _channel.BasicAck(e.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing message from queue {Queue}",
                    queueName);

                _channel.BasicNack(
                    deliveryTag: e.DeliveryTag,
                    multiple: false,
                    requeue: false);
            }
        };

        var consumerTag = _channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer);
        _consumerTags[queueName] = consumerTag;
    }

    public void DeclareExchange(string exchangeName, string type)
    {
        _channel.ExchangeDeclare(exchangeName, type, durable: true);
    }

    public void DeclareQueue(string queueName)
    {
        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public void BindQueue(
        string queueName,
        string exchangeName,
        string routingKey)
    {
        _channel.QueueBind(
            queue: queueName,
            exchange: exchangeName,
            routingKey: routingKey);
    }

    public void PurgeQueue(string queueName)
    {
        _channel.QueuePurge(queueName);
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
        }
        catch (Exception)
        {
            // Ignore exceptions during channel close
        }

        try
        {
            _connection?.Close();
        }
        catch (Exception)
        {
            // Ignore exceptions during connection close
        }
    }

    public void Unsubscribe(string queueName)
    {
        if (_consumers.TryGetValue(queueName, out var consumer))
        {
            if (_consumerTags.TryGetValue(queueName, out var consumerTag))
            {
                _channel.BasicCancel(consumerTag);
                _consumerTags.Remove(queueName);
            }
            _consumers.Remove(queueName);
            _handlers.Remove(queueName);
        }
    }

    public void DeleteQueue(string queueName)
    {
        _channel.QueueDelete(queueName);
    }
}
