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
    private readonly IProviderSimulationContext _provider;

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
        _provider = providerLogic;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await SetupRabbitMq(cancellationToken);
        await PublishStartupEvent(cancellationToken);
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private async Task PublishStartupEvent(CancellationToken cancellationToken)
    {
        await Task.Delay(3000, cancellationToken); // Ensure everyone is ready to receive the message
        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.ProviderStarted, new ProviderStartedEvent
        {
            Provider = _provider.Provider
        });
    }

    private async Task SetupRabbitMq(CancellationToken cancellationToken)
    {
        var proposalQueueName = $"simulator.process.proposal.{_provider.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(proposalQueueName);
        _messagingInfrastructure.BindQueue(proposalQueueName, Exchanges.Process, proposalQueueName);
        _messagingInfrastructure.PurgeQueue(proposalQueueName);
        _messageSubscriber.Subscribe<ProposeProcessToProviderCommand>(proposalQueueName, e => _dispatcher.DispatchAsync(e));

        var confirmationQueueName = $"simulator.process.confirm.{_provider.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(confirmationQueueName);
        _messagingInfrastructure.BindQueue(confirmationQueueName, Exchanges.Process, confirmationQueueName);
        _messagingInfrastructure.PurgeQueue(confirmationQueueName);
        _messageSubscriber.Subscribe<ConfirmProcessProposalCommand>(confirmationQueueName, e => _dispatcher.DispatchAsync(e));

        var updateProviderQueueName = $"simulator.provider.update-provider.{_provider.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(updateProviderQueueName);
        _messagingInfrastructure.BindQueue(updateProviderQueueName, Exchanges.Provider, ProviderRoutingKeys.UpdateProvider);
        _messagingInfrastructure.PurgeQueue(updateProviderQueueName);
        _messageSubscriber.Subscribe<UpdateProviderCommand>(updateProviderQueueName, e => _dispatcher.DispatchAsync(e));

        var requestScheduleQueueName = $"simulator.provider.request-schedule.{_provider.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(requestScheduleQueueName);
        _messagingInfrastructure.BindQueue(requestScheduleQueueName, Exchanges.Provider, ProviderRoutingKeys.RequestProviderSchedule);
        _messagingInfrastructure.PurgeQueue(requestScheduleQueueName);
        _messageSubscriber.Subscribe<RequestProviderScheduleCommand>(requestScheduleQueueName, e => _dispatcher.DispatchAsync(e));

        // NEW: Subscribe to Execution Commands
        // We bind a unique queue for this simulator to the routing key the Engine uses
        var executionQueueName = $"simulator.process.execute.{_provider.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(executionQueueName);
        _messagingInfrastructure.BindQueue(executionQueueName, Exchanges.Process, $"process.execute.{_provider.Provider.Id}");
        _messagingInfrastructure.PurgeQueue(executionQueueName);
        _messageSubscriber.Subscribe<ExecuteProcessCommand>(executionQueueName, e => _dispatcher.DispatchAsync(e));

        await Task.Delay(1000, cancellationToken);
    }
}