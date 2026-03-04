using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;

namespace ManufacturingOptimization.Gateway.Handlers;

public class SystemReadyHandler : IMessageHandler<SystemReadyEvent>
{
    private readonly ISystemReadinessService _systemReadinessService;
    private readonly INotificationPublisher _notificationPublisher;

    public SystemReadyHandler(
        ISystemReadinessService systemReadinessService,
        INotificationPublisher notificationPublisher)
    {
        _systemReadinessService = systemReadinessService;
        _notificationPublisher = notificationPublisher;
    }

    public Task HandleAsync(SystemReadyEvent evt)
    {
        _systemReadinessService.MarkSystemReady();
        
        // Notify that system is fully ready
        _notificationPublisher.NotifySystemReady();
        
        return Task.CompletedTask;
    }
}