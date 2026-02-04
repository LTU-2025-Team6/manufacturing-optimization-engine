using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagement;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;
using ManufacturingOptimization.Engine.Handlers;
using System.Threading.Tasks;

namespace ManufacturingOptimization.Engine;

public class EngineWorker : BackgroundService
{
    private readonly ILogger<EngineWorker> _logger;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IMessageDispatcher _dispatcher;

    public EngineWorker(
        ILogger<EngineWorker> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IMessageDispatcher dispatcher)
    {
        _logger = logger;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;
        _dispatcher = dispatcher;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await SetupRabbitMq(cancellationToken);
        PublishStartupEvent();
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private async Task SetupRabbitMq(CancellationToken cancellationToken)
    {
        _messagingInfrastructure.DeclareQueue("engine.system.service-ready");
        _messagingInfrastructure.BindQueue("engine.system.service-ready", Exchanges.System, SystemRoutingKeys.ServiceReady);
        _messagingInfrastructure.PurgeQueue("engine.system.service-ready");
        _messageSubscriber.Subscribe<ServiceReadyEvent>("engine.system.service-ready", e => _dispatcher.DispatchAsync(e));

        _messagingInfrastructure.DeclareQueue("engine.system.ready");
        _messagingInfrastructure.BindQueue("engine.system.ready", Exchanges.System, SystemRoutingKeys.SystemReady);
        _messagingInfrastructure.PurgeQueue("engine.system.ready");
        _messageSubscriber.Subscribe<SystemReadyEvent>("engine.system.ready", e => _dispatcher.DispatchAsync(e));

        _messagingInfrastructure.DeclareQueue("engine.providers.ready");
        _messagingInfrastructure.BindQueue("engine.providers.ready", Exchanges.Provider, ProviderRoutingKeys.AllProvidersStarted);
        _messagingInfrastructure.PurgeQueue("engine.providers.ready");
        _messageSubscriber.Subscribe<AllProvidersStartedEvent>("engine.providers.ready", e => _dispatcher.DispatchAsync(e));

        _messagingInfrastructure.DeclareQueue("engine.provider.events");
        _messagingInfrastructure.BindQueue("engine.provider.events", Exchanges.Provider, ProviderRoutingKeys.ProviderStarted);
        _messagingInfrastructure.BindQueue("engine.provider.events", Exchanges.Provider, ProviderRoutingKeys.ProviderStopped);
        _messagingInfrastructure.BindQueue("engine.provider.events", Exchanges.Provider, ProviderRoutingKeys.ProviderUpdated);
        _messagingInfrastructure.PurgeQueue("engine.provider.events");
        _messageSubscriber.Subscribe<ProviderStartedEvent>("engine.provider.events", e => _dispatcher.DispatchAsync(e));
        _messageSubscriber.Subscribe<ProviderStoppedEvent>("engine.provider.events", e => _dispatcher.DispatchAsync(e));
        _messageSubscriber.Subscribe<ProviderUpdatedEvent>("engine.provider.events", e => _dispatcher.DispatchAsync(e));

        _messagingInfrastructure.DeclareQueue("engine.optimization.requests");
        _messagingInfrastructure.BindQueue("engine.optimization.requests", Exchanges.Optimization, OptimizationRoutingKeys.PlanRequested);
        _messagingInfrastructure.PurgeQueue("engine.optimization.requests");
        _messageSubscriber.Subscribe<RequestOptimizationPlanCommand>("engine.optimization.requests", e => _dispatcher.DispatchAsync(e));

        await Task.Delay(1000, cancellationToken);
    }

    private void PublishStartupEvent()
    {
        _messagePublisher.Publish(Exchanges.System, SystemRoutingKeys.ServiceReady, new ServiceReadyEvent
        {
            ServiceName = "Engine"
        });
    }
}