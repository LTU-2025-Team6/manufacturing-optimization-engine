using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;

namespace ManufacturingOptimization.Gateway.Handlers;

public class AllProvidersStartedHandler : IMessageHandler<AllProvidersStartedEvent>
{
    private readonly ISystemReadinessService _systemReadinessService;
    private readonly INotificationPublisher _notificationPublisher;

    public AllProvidersStartedHandler(
        ISystemReadinessService systemReadinessService,
        INotificationPublisher notificationPublisher)
    {
        _systemReadinessService = systemReadinessService;
        _notificationPublisher = notificationPublisher;
    }

    public Task HandleAsync(AllProvidersStartedEvent evt)
    {
        _systemReadinessService.MarkProvidersReady();

        _notificationPublisher.NotifyAllProvidersStarted();

        return Task.CompletedTask;
    }
}
