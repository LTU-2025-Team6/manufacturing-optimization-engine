using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using System.Threading.Tasks;

namespace ManufacturingOptimization.ProviderSimulator;

public class ProviderSimulatorWorker : BackgroundService
{
    private readonly ILogger<ProviderSimulatorWorker> _logger;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IMessageDispatcher _dispatcher;
    private readonly IProviderSimulationContext _providerContext;

    public ProviderSimulatorWorker(
        ILogger<ProviderSimulatorWorker> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IMessageDispatcher dispatcher,
        IProviderSimulationContext providerLogic)
    {
        _logger = logger;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;
        _dispatcher = dispatcher;
        _providerContext = providerLogic;
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

        var executionQueueName = $"simulator.process.execute.{_providerContext.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(executionQueueName);
        _messagingInfrastructure.BindQueue(executionQueueName, Exchanges.Process, $"process.execute.{_providerContext.Provider.Id}");
        _messagingInfrastructure.PurgeQueue(executionQueueName);
        _messageSubscriber.Subscribe<ExecuteProcessCommand>(executionQueueName, e => _dispatcher.DispatchAsync(e));

        await Task.Delay(300, cancellationToken);
    }
}