using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;
using ManufacturingOptimization.Gateway.Abstractions;

namespace ManufacturingOptimization.Gateway.Services;

public class GatewayWorker : BackgroundService
{
    private readonly ILogger<GatewayWorker> _logger;
    private readonly IProviderOrchestrator _orchestrator;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IMessageDispatcher _dispatcher;
    private readonly ISystemReadinessService _readinessService;

    public GatewayWorker(
        ILogger<GatewayWorker> logger,
        IProviderOrchestrator orchestrator,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IMessageDispatcher dispatcher,
        ISystemReadinessService readinessService)
    {
        _logger = logger;
        _orchestrator = orchestrator;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;
        _dispatcher = dispatcher;
        _readinessService = readinessService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await SetupRabbitMq(cancellationToken);
        PublishStartupEvent();
        await _readinessService.WaitForSystemReadyAsync(cancellationToken);
        PublishStartProviders();
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private async Task SetupRabbitMq(CancellationToken cancellationToken)
    {
        _messagingInfrastructure.DeclareQueue("gateway.system.ready");
        _messagingInfrastructure.BindQueue("gateway.system.ready", Exchanges.System, SystemRoutingKeys.SystemReady);
        _messagingInfrastructure.PurgeQueue("gateway.system.ready");
        _messageSubscriber.Subscribe<SystemReadyEvent>("gateway.system.ready", e => _dispatcher.DispatchAsync(e));

        _messagingInfrastructure.DeclareQueue("gateway.providers.ready");
        _messagingInfrastructure.BindQueue("gateway.providers.ready", Exchanges.Provider, ProviderRoutingKeys.AllProvidersStarted);
        _messagingInfrastructure.PurgeQueue("gateway.providers.ready");
        _messageSubscriber.Subscribe<AllProvidersStartedEvent>("gateway.providers.ready", e => _dispatcher.DispatchAsync(e));

        _messagingInfrastructure.DeclareQueue("gateway.provider.events");
        _messagingInfrastructure.BindQueue("gateway.provider.events", Exchanges.Provider, ProviderRoutingKeys.StartAllProviders);
        _messagingInfrastructure.BindQueue("gateway.provider.events", Exchanges.Provider, ProviderRoutingKeys.StopAllProviders);
        _messagingInfrastructure.BindQueue("gateway.provider.events", Exchanges.Provider, ProviderRoutingKeys.AllProvidersStopped);
        _messagingInfrastructure.BindQueue("gateway.provider.events", Exchanges.Provider, ProviderRoutingKeys.StartProvider);
        _messagingInfrastructure.BindQueue("gateway.provider.events", Exchanges.Provider, ProviderRoutingKeys.ProviderStarted);
        _messagingInfrastructure.BindQueue("gateway.provider.events", Exchanges.Provider, ProviderRoutingKeys.StopProvider);
        _messagingInfrastructure.BindQueue("gateway.provider.events", Exchanges.Provider, ProviderRoutingKeys.ProviderStopped);
        _messagingInfrastructure.PurgeQueue("gateway.provider.events");
        _messageSubscriber.Subscribe<StartAllProvidersCommand>("gateway.provider.events", e => _dispatcher.DispatchAsync(e));
        _messageSubscriber.Subscribe<StopAllProvidersCommand>("gateway.provider.events", e => _dispatcher.DispatchAsync(e));
        _messageSubscriber.Subscribe<AllProvidersStoppedEvent>("gateway.provider.events", e => _dispatcher.DispatchAsync(e));
        _messageSubscriber.Subscribe<StartProviderCommand>("gateway.provider.events", e => _dispatcher.DispatchAsync(e));
        _messageSubscriber.Subscribe<ProviderStartedEvent>("gateway.provider.events", e => _dispatcher.DispatchAsync(e));
        _messageSubscriber.Subscribe<StopProviderCommand>("gateway.provider.events", e => _dispatcher.DispatchAsync(e));
        _messageSubscriber.Subscribe<ProviderStoppedEvent>("gateway.provider.events", e => _dispatcher.DispatchAsync(e));

        _messagingInfrastructure.DeclareQueue("gateway.optimization.plan-updated");
        _messagingInfrastructure.BindQueue("gateway.optimization.plan-updated", Exchanges.Optimization, OptimizationRoutingKeys.PlanUpdated);
        _messagingInfrastructure.PurgeQueue("gateway.optimization.plan-updated");
        _messageSubscriber.Subscribe<OptimizationPlanUpdatedEvent>("gateway.optimization.plan-updated", e => _dispatcher.DispatchAsync(e));

        // Give subscriptions time to register
        await Task.Delay(300, cancellationToken);
    }

    private void PublishStartupEvent()
    {
        _messagePublisher.Publish(Exchanges.System, SystemRoutingKeys.ServiceReady, new ServiceReadyEvent
        {
            ServiceName = "Gateway"
        });
    }

    private void PublishStartProviders()
    {
        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.StartAllProviders, new StartAllProvidersCommand());
    }
}