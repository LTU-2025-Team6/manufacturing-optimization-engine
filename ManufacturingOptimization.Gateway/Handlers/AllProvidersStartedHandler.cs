using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;

namespace ManufacturingOptimization.Gateway.Handlers;

public class AllProvidersStartedHandler : IMessageHandler<AllProvidersStartedEvent>
{
    private readonly ISystemReadinessService _systemReadinessService;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ISimulationClock _clock;
    private readonly INotificationPublisher _notificationPublisher;

    public AllProvidersStartedHandler(
        IMessagePublisher messagePublisher,
        ISimulationClock clock,
        ISystemReadinessService systemReadinessService,
        INotificationPublisher notificationPublisher)
    {
        _messagePublisher = messagePublisher;
        _clock = clock;
        _systemReadinessService = systemReadinessService;
        _notificationPublisher = notificationPublisher;
    }

    public Task HandleAsync(AllProvidersStartedEvent evt)
    {
        _systemReadinessService.MarkProvidersReady();

        _notificationPublisher.NotifyAllProvidersStarted();

        // Broadcast the current simulation time so all providers sync their clocks immediately.
        // Gateway always has a valid time (defaults to real UTC on first start, or restores from DB).
        _messagePublisher.Publish(Exchanges.System, SystemRoutingKeys.TimeChanged,
            new SimulationTimeChangedEvent
            {
                SimulatedUtcNow = _clock.UtcNow,
                SpeedMultiplier = _clock.SpeedMultiplier
            });

        return Task.CompletedTask;
    }
}
