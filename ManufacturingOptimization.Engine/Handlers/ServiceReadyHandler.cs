using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;

namespace ManufacturingOptimization.Engine.Handlers;

public class ServiceReadyHandler : IMessageHandler<ServiceReadyEvent>
{
    private readonly List<string> REQUIRED_SERVICES = new()
    {
        "Gateway",
        "Engine"
    };
    private readonly IMessagePublisher _messagePublisher;
    private readonly ISystemReadinessService _systemReadinessService;

    private readonly static HashSet<string> _readyServices = new();
    private bool _systemReadyPublished = false;

    public ServiceReadyHandler(
        ISystemReadinessService systemReadinessService,
        IMessagePublisher messagePublisher)
    {
        _systemReadinessService = systemReadinessService;
        _messagePublisher = messagePublisher;
    }

    public Task HandleAsync(ServiceReadyEvent evt)
    {
        lock (_readyServices)
        {
            _readyServices.Add(evt.ServiceName);
            CheckAndPublishSystemReady();
        }
        return Task.CompletedTask;
    }

    private void CheckAndPublishSystemReady()
    {
        if (_systemReadyPublished)
            return;

        var allReady = REQUIRED_SERVICES.All(s => _readyServices.Contains(s));

        if (allReady)
        {
            var evt = new SystemReadyEvent
            {
                ReadyServices = _readyServices.ToList()
            };

            _messagePublisher.Publish(Exchanges.System, SystemRoutingKeys.SystemReady, evt);
            _systemReadyPublished = true;
        }
    }
}
