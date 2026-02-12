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
        _messagingInfrastructure.DeclareQueue($"simulator.process.proposal.{_provider.Provider.Id}");
        _messagingInfrastructure.BindQueue($"simulator.process.proposal.{_provider.Provider.Id}", Exchanges.Process, $"{ProcessRoutingKeys.Propose}.{_provider.Provider.Id}");
        _messagingInfrastructure.PurgeQueue($"simulator.process.proposal.{_provider.Provider.Id}");
        _messageSubscriber.Subscribe<ProposeProcessToProviderCommand>($"simulator.process.proposal.{_provider.Provider.Id}", e => _dispatcher.DispatchAsync(e));

        _messagingInfrastructure.DeclareQueue($"simulator.process.confirm.{_provider.Provider.Id}");
        _messagingInfrastructure.BindQueue($"simulator.process.confirm.{_provider.Provider.Id}", Exchanges.Process, $"{ProcessRoutingKeys.Confirm}.{_provider.Provider.Id}");
        _messagingInfrastructure.PurgeQueue($"simulator.process.confirm.{_provider.Provider.Id}");
        _messageSubscriber.Subscribe<ConfirmProcessProposalCommand>($"simulator.process.confirm.{_provider.Provider.Id}", e => _dispatcher.DispatchAsync(e));

        _messagingInfrastructure.DeclareQueue("simulator.provider.update-provider");
        _messagingInfrastructure.BindQueue("simulator.provider.update-provider", Exchanges.Provider, ProviderRoutingKeys.UpdateProvider);
        _messagingInfrastructure.PurgeQueue("simulator.provider.update-provider");
        _messageSubscriber.Subscribe<UpdateProviderCommand>("simulator.provider.update-provider", e => _dispatcher.DispatchAsync(e));

        _messagingInfrastructure.DeclareQueue("simulator.provider.request-schedule");
        _messagingInfrastructure.BindQueue("simulator.provider.request-schedule", Exchanges.Provider, ProviderRoutingKeys.RequestProviderSchedule);
        _messagingInfrastructure.PurgeQueue("simulator.provider.request-schedule");
        _messageSubscriber.Subscribe<RequestProviderScheduleCommand>("simulator.provider.request-schedule", e => _dispatcher.DispatchAsync(e));

        await Task.Delay(1000, cancellationToken);
    }
}