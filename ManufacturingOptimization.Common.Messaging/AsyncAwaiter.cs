using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging;

public sealed class AsyncAwaiter : IAsyncAwaiter
{
    private readonly IMessagingInfrastructure _infrastructure;
    private readonly IMessageSubscriber _subscriber;

    public AsyncAwaiter(
        IMessagingInfrastructure infra,
        IMessageSubscriber subscriber)
    {
        _infrastructure = infra;
        _subscriber = subscriber;
    }

    public async Task<TEvent> AwaitAsync<TEvent>(AwaitScenario<TEvent> scenario) where TEvent : IMessage
    {
        var tcs = new TaskCompletionSource<TEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
        var queueName = $"await.{typeof(TEvent).Name}.{Guid.NewGuid()}";

        _infrastructure.DeclareQueue(queueName);
        _infrastructure.BindQueue(queueName, scenario.Exchange, scenario.RoutingKey);

        Action<TEvent> handler = evt =>
        {
            if (scenario.Match(evt))
                tcs.TrySetResult(evt);
        };
            
        _subscriber.Subscribe(queueName, handler);

        try
        {
            scenario.BeforeAwait?.Invoke();

            using var cts = new CancellationTokenSource(scenario.Timeout);
            var completed = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, cts.Token));

            if (completed != tcs.Task)
                throw new TimeoutException($"Timeout waiting for {typeof(TEvent).Name}");

            scenario.AfterMatch?.Invoke();
            return await tcs.Task;
        }
        finally
        {
            _subscriber.Unsubscribe(queueName);
            _infrastructure.DeleteQueue(queueName);
        }
    }

    public async Task<IReadOnlyList<TEvent>> AwaitMultipleAsync<TEvent>(AwaitMultipleScenario<TEvent> scenario) where TEvent : IMessage
    {
        var responses = new List<TEvent>();
        var lockObj = new object();
        var tcs = new TaskCompletionSource<IReadOnlyList<TEvent>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var queueName = $"await-multi.{typeof(TEvent).Name}.{Guid.NewGuid()}";

        _infrastructure.DeclareQueue(queueName);
        _infrastructure.BindQueue(queueName, scenario.Exchange, scenario.RoutingKey);

        Action<TEvent> handler = evt =>
        {
            bool shouldComplete = false;
            IReadOnlyList<TEvent>? result = null;

            lock (lockObj)
            {
                if (tcs.Task.IsCompleted)
                    return;

                if (scenario.Match(evt))
                {
                    responses.Add(evt);
                    
                    if (scenario.CompletionCondition(responses))
                    {
                        shouldComplete = true;
                        result = responses.ToList();
                    }
                }
            }

            if (shouldComplete && result != null)
                tcs.TrySetResult(result);
        };

        _subscriber.Subscribe(queueName, handler);

        try
        {
            scenario.BeforeAwait?.Invoke();

            using var cts = new CancellationTokenSource(scenario.Timeout);
            var timeoutTask = Task.Delay(Timeout.Infinite, cts.Token);
            var completed = await Task.WhenAny(tcs.Task, timeoutTask);

            IReadOnlyList<TEvent> finalResult;
            
            if (completed == timeoutTask)
            {
                // Timeout - return whatever we have, or empty if nothing matched
                lock (lockObj)
                {
                    finalResult = responses.ToList();
                }
            }
            else
            {
                finalResult = await tcs.Task;
            }

            scenario.AfterCompletion?.Invoke(finalResult);
            return finalResult;
        }
        finally
        {
            _subscriber.Unsubscribe(queueName);
            _infrastructure.DeleteQueue(queueName);
        }
    }
}