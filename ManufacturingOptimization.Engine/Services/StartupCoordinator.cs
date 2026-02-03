using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManufacturingOptimization.Engine.Services;

/// <summary>
/// Extends SystemReadinessService with startup coordination logic:
/// Waits for all services to report readiness (Gateway, ProviderRegistry, Engine)
/// Publishes SystemReadyEvent when all are ready
/// Listens for SystemReadyEvent and marks itself ready (inherited behavior)
/// </summary>
public class StartupCoordinator : SystemReadinessService
{
    private readonly List<string> REQUIRED_SERVICES = new() 
    { 
        "Gateway", 
        "Engine"
    };
    private readonly IMessagePublisher _messagePublisher;
    
    private readonly HashSet<string> _readyServices = new();
    private bool _systemReadyPublished = false;

    public StartupCoordinator(
        ILogger<StartupCoordinator> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IOptions<SystemReadinessSettings> options)
        : base(logger, messagingInfrastructure, messageSubscriber, options)
    {
        _messagePublisher = messagePublisher;
    }

    protected override void SetupRabbitMq()
    {
        // Call base to setup SystemReadyEvent listener
        base.SetupRabbitMq();
        
        _messagingInfrastructure.DeclareQueue("coordinator.service.ready");
        _messagingInfrastructure.BindQueue("coordinator.service.ready", Exchanges.System, SystemRoutingKeys.ServiceReady);
        _messagingInfrastructure.PurgeQueue("coordinator.service.ready");
        _messageSubscriber.Subscribe<ServiceReadyEvent>("coordinator.service.ready", HandleServiceReady);
    }

    private void HandleServiceReady(ServiceReadyEvent evt)
    {
        lock (_readyServices)
        {
            _readyServices.Add(evt.ServiceName);
            CheckAndPublishSystemReady();
        }
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
