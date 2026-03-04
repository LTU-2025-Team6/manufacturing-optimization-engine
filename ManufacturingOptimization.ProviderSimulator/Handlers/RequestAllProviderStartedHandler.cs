using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.ProviderSimulator.Abstractions;

namespace ManufacturingOptimization.ProviderSimulator.Handlers;

public sealed class RequestAllProviderStartedHandler : IMessageHandler<RequestAllProviderStartedCommand>
{
    private readonly IProviderSimulationContext _providerContext;
    private readonly IMessagePublisher _messagePublisher;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly ILogger<RequestAllProviderStartedHandler> _logger;

    public RequestAllProviderStartedHandler(
        IProviderSimulationContext providerContext,
        IMessagePublisher messagePublisher,
        INotificationPublisher notificationPublisher,
        ILogger<RequestAllProviderStartedHandler> logger)
    {
        _providerContext = providerContext;
        _messagePublisher = messagePublisher;
        _notificationPublisher = notificationPublisher;
        _logger = logger;
    }

    public Task HandleAsync(RequestAllProviderStartedCommand command)
    {
        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.ProviderStarted, new ProviderStartedEvent
        {
            Provider = _providerContext.Provider
        });

        return Task.CompletedTask;
    }
}