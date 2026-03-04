using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.ProviderSimulator.Abstractions;

namespace ManufacturingOptimization.ProviderSimulator;

public class ProviderSimulatorWorker : BackgroundService
{
    private readonly ILogger<ProviderSimulatorWorker> _logger;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IMessageDispatcher _dispatcher;
    private readonly IProviderSimulationContext _providerContext;
    private readonly ISimulationClock _clock;

    public ProviderSimulatorWorker(
        ILogger<ProviderSimulatorWorker> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IMessageDispatcher dispatcher,
        IProviderSimulationContext providerLogic,
        ISimulationClock clock)
    {
        _logger = logger;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;
        _dispatcher = dispatcher;
        _providerContext = providerLogic;
        _clock = clock;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await SetupRabbitMq(cancellationToken);
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private async Task SetupRabbitMq(CancellationToken cancellationToken)
    {
        var requestAllProviderStartedQueue = $"simulator.provider.request-start.{_providerContext.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(requestAllProviderStartedQueue);
        _messagingInfrastructure.BindQueue(requestAllProviderStartedQueue, Exchanges.Provider, ProviderRoutingKeys.RequestAllProviderStarted);
        _messagingInfrastructure.PurgeQueue(requestAllProviderStartedQueue);
        _messageSubscriber.Subscribe<RequestAllProviderStartedCommand>(requestAllProviderStartedQueue, e => _dispatcher.DispatchAsync(e));

        var requestProviderStartedQueue = $"simulator.provider.request-single-start.{_providerContext.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(requestProviderStartedQueue);
        _messagingInfrastructure.BindQueue(requestProviderStartedQueue, Exchanges.Provider, ProviderRoutingKeys.RequestProviderStarted);
        _messagingInfrastructure.PurgeQueue(requestProviderStartedQueue);
        _messageSubscriber.Subscribe<RequestProviderStartedCommand>(requestProviderStartedQueue, e => _dispatcher.DispatchAsync(e));

        var proposeQueue = $"simulator.process.proposal.{_providerContext.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(proposeQueue);
        _messagingInfrastructure.BindQueue(proposeQueue, Exchanges.Process, $"{ProcessRoutingKeys.Propose}.{_providerContext.Provider.Id}");
        _messagingInfrastructure.PurgeQueue(proposeQueue);
        _messageSubscriber.Subscribe<ProposeProcessToProviderCommand>(proposeQueue, e => _dispatcher.DispatchAsync(e));

        var confirmQueue = $"simulator.process.confirm.{_providerContext.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(confirmQueue);
        _messagingInfrastructure.BindQueue(confirmQueue, Exchanges.Process, $"{ProcessRoutingKeys.Confirm}.{_providerContext.Provider.Id}");
        _messagingInfrastructure.PurgeQueue(confirmQueue);
        _messageSubscriber.Subscribe<ConfirmProcessProposalCommand>(confirmQueue, e => _dispatcher.DispatchAsync(e));

        var cancelQueue = $"simulator.process.cancel.{_providerContext.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(cancelQueue);
        _messagingInfrastructure.BindQueue(cancelQueue, Exchanges.Process, $"{ProcessRoutingKeys.Cancel}.{_providerContext.Provider.Id}");
        _messagingInfrastructure.PurgeQueue(cancelQueue);
        _messageSubscriber.Subscribe<CancelProcessCommand>(cancelQueue, e => _dispatcher.DispatchAsync(e));

        var updateProviderQueue = $"simulator.provider.update-provider.{_providerContext.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(updateProviderQueue);
        _messagingInfrastructure.BindQueue(updateProviderQueue, Exchanges.Provider, ProviderRoutingKeys.UpdateProvider);
        _messagingInfrastructure.PurgeQueue(updateProviderQueue);
        _messageSubscriber.Subscribe<UpdateProviderCommand>(updateProviderQueue, e => _dispatcher.DispatchAsync(e));

        var requestScheduleQueue = $"simulator.provider.request-schedule.{_providerContext.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(requestScheduleQueue);
        _messagingInfrastructure.BindQueue(requestScheduleQueue, Exchanges.Provider, ProviderRoutingKeys.RequestProviderSchedule);
        _messagingInfrastructure.PurgeQueue(requestScheduleQueue);
        _messageSubscriber.Subscribe<RequestProviderScheduleCommand>(requestScheduleQueue, e => _dispatcher.DispatchAsync(e));

        var requestExecutionDetailsQueue = $"simulator.provider.request-execution-details.{_providerContext.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(requestExecutionDetailsQueue);
        _messagingInfrastructure.BindQueue(requestExecutionDetailsQueue, Exchanges.Provider, ProviderRoutingKeys.RequestExecutionDetails);
        _messagingInfrastructure.PurgeQueue(requestExecutionDetailsQueue);
        _messageSubscriber.Subscribe<RequestExecutionDetailsCommand>(requestExecutionDetailsQueue, e => _dispatcher.DispatchAsync(e));

        // Subscribe to simulation time changes from Gateway
        var timeChangedQueue = $"simulator.system.time-changed.{_providerContext.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(timeChangedQueue);
        _messagingInfrastructure.BindQueue(timeChangedQueue, Exchanges.System, SystemRoutingKeys.TimeChanged);
        _messagingInfrastructure.PurgeQueue(timeChangedQueue);
        _messageSubscriber.Subscribe<SimulationTimeChangedEvent>(timeChangedQueue, e =>
        {
            _clock.SetTime(e.SimulatedUtcNow, e.SpeedMultiplier);
        });

        await Task.Delay(300, cancellationToken);
    }
}