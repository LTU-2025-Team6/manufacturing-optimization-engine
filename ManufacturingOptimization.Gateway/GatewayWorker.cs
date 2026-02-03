using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
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

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _orchestrator.CleanupOrphanedContainersAsync(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _orchestrator.StopAllAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await SetupRabbitMq(cancellationToken);
        PublishReady();
        await _readinessService.WaitForSystemReadyAsync(cancellationToken);
        await StartProviders(cancellationToken);
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private async Task SetupRabbitMq(CancellationToken cancellationToken)
    {
        _messagingInfrastructure.DeclareQueue("gateway.plan.status.updates");
        _messagingInfrastructure.BindQueue("gateway.plan.status.updates", Exchanges.Optimization, OptimizationRoutingKeys.PlanUpdated);
        _messagingInfrastructure.PurgeQueue("gateway.plan.status.updates");
        _messageSubscriber.Subscribe<OptimizationPlanUpdatedEvent>("gateway.plan.status.updates", e => _dispatcher.DispatchAsync(e));

        _messagingInfrastructure.DeclareQueue("orchestrator.provider.registered");
        _messagingInfrastructure.BindQueue("orchestrator.provider.registered", Exchanges.Provider, ProviderRoutingKeys.Registered);
        _messagingInfrastructure.PurgeQueue("orchestrator.provider.registered");
        _messageSubscriber.Subscribe<ProviderRegisteredEvent>("orchestrator.provider.registered", e => _dispatcher.DispatchAsync(e));

        // Give subscriptions time to register
        await Task.Delay(1000, cancellationToken);
    }

    private void PublishReady()
    {
        _messagePublisher.Publish(Exchanges.System, SystemRoutingKeys.ServiceReady, new ServiceReadyEvent
        {
            ServiceName = "Gateway"
        });
    }

    private async Task StartProviders(CancellationToken cancellationToken)
    {
        await _orchestrator.StartAllAsync(cancellationToken);
        await Task.Delay(3000, cancellationToken); // Important: wait a moment before requesting registrations to ensure all containers are ready
        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.RequestRegistrationAll, new RequestProvidersRegistrationCommand());
    }
}