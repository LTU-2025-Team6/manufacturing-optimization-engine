using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.ProviderSimulator.Abstractions;

namespace ManufacturingOptimization.ProviderSimulator.Handlers;

public sealed class UpdateProviderHandler : IMessageHandler<UpdateProviderCommand>
{
    private readonly IProviderSimulationContext _providerContext;
    private readonly IMessagePublisher _messagePublisher;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly ILogger<UpdateProviderHandler> _logger;

    public UpdateProviderHandler(
        IProviderSimulationContext providerContext,
        IMessagePublisher messagePublisher,
        INotificationPublisher notificationPublisher,
        ILogger<UpdateProviderHandler> logger)
    {
        _providerContext = providerContext;
        _messagePublisher = messagePublisher;
        _notificationPublisher = notificationPublisher;
        _logger = logger;
    }

    public Task HandleAsync(UpdateProviderCommand command)
    {
        if (command.Provider.Id != _providerContext.Provider.Id)
            return Task.CompletedTask;

        // Notify requested update
        _notificationPublisher.NotifyProviderUpdateRequested(_providerContext.Provider.Name);

        _providerContext.Provider = command.Provider;
        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.ProviderUpdated, new ProviderUpdatedEvent
        {
            Provider = _providerContext.Provider
        });

        // Notify request completion
        _notificationPublisher.NotifyProviderUpdated(_providerContext.Provider.Name);

        return Task.CompletedTask;
    }
}
