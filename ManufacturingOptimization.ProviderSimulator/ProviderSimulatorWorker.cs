using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.ProviderSimulator.Abstractions;

namespace ManufacturingOptimization.ProviderSimulator;

public class ProviderSimulatorWorker : BackgroundService
{
    private readonly ILogger<ProviderSimulatorWorker> _logger;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessageDispatcher _dispatcher;
    private readonly IProviderSimulationContext _providerLogic;

    public ProviderSimulatorWorker(
        ILogger<ProviderSimulatorWorker> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessageDispatcher dispatcher,
        IProviderSimulationContext providerLogic)
    {
        _logger = logger;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _dispatcher = dispatcher;
        _providerLogic = providerLogic;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SetupRabbitMq();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void SetupRabbitMq()
    {
        // Listen to process proposals for this specific provider
        var proposalQueueName = $"process.proposal.{_providerLogic.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(proposalQueueName);
        _messagingInfrastructure.BindQueue(proposalQueueName, Exchanges.Process, proposalQueueName);
        _messagingInfrastructure.PurgeQueue(proposalQueueName);
        _messageSubscriber.Subscribe<ProposeProcessToProviderCommand>(proposalQueueName, e => _dispatcher.DispatchAsync(e));

        // Listen to confirmations for this specific provider
        var confirmationQueueName = $"process.confirm.{_providerLogic.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(confirmationQueueName);
        _messagingInfrastructure.BindQueue(confirmationQueueName, Exchanges.Process, confirmationQueueName);
        _messagingInfrastructure.PurgeQueue(confirmationQueueName);
        _messageSubscriber.Subscribe<ConfirmProcessProposalCommand>(confirmationQueueName, e => _dispatcher.DispatchAsync(e));

        // Listen to process execution requests
        var executionQueueName = $"process.execute.{_providerLogic.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(executionQueueName);
        _messagingInfrastructure.BindQueue(executionQueueName, Exchanges.Process, executionQueueName);
        _messagingInfrastructure.PurgeQueue(executionQueueName);
        _messageSubscriber.Subscribe<ExecuteProcessCommand>(executionQueueName, e => _dispatcher.DispatchAsync(e));

        // Listen to provider coordination commands
        var providerCoordinationQueue = $"provider.coordination.{_providerLogic.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(providerCoordinationQueue);
        _messagingInfrastructure.BindQueue(providerCoordinationQueue, Exchanges.Provider, ProviderRoutingKeys.RequestRegistrationAll);
        _messagingInfrastructure.PurgeQueue(providerCoordinationQueue);
        _messageSubscriber.Subscribe<RequestProvidersRegistrationCommand>(providerCoordinationQueue, e => _dispatcher.DispatchAsync(e));
    }
}