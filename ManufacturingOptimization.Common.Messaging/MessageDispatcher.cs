using ManufacturingOptimization.Common.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ManufacturingOptimization.Common.Messaging;

/// <summary>
/// Dispatches messages to their registered handlers using scoped service resolution.
/// This allows singleton services (like BackgroundService) to invoke scoped handlers.
/// </summary>
public class MessageDispatcher : IMessageDispatcher
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MessageDispatcher> _logger;

    public MessageDispatcher(
        IServiceScopeFactory scopeFactory,
        ILogger<MessageDispatcher> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Dispatches a message to its registered handler within a new service scope.
    /// </summary>
    /// <typeparam name="TMessage">The type of message to dispatch</typeparam>
    /// <param name="message">The message to dispatch</param>
    public async Task DispatchAsync<TMessage>(TMessage message)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TMessage>>();

            await handler.HandleAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching message of type {MessageType}", typeof(TMessage).Name);
            throw;
        }
    }
}
