using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.ProviderSimulator.Abstractions;

namespace ManufacturingOptimization.ProviderSimulator.Handlers;

public sealed class RequestProviderStartedHandler : IMessageHandler<RequestProviderStartedCommand>
{
    private readonly IProviderSimulationContext _providerContext;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<RequestProviderStartedHandler> _logger;

    public RequestProviderStartedHandler(
        IProviderSimulationContext providerContext,
        IMessagePublisher messagePublisher,
        ILogger<RequestProviderStartedHandler> logger)
    {
        _providerContext = providerContext;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public Task HandleAsync(RequestProviderStartedCommand command)
    {
        if (command.ProviderId != _providerContext.Provider.Id)
            return Task.CompletedTask;

        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.ProviderStarted, new ProviderStartedEvent
        {
            Provider = _providerContext.Provider
        });

        return Task.CompletedTask;
    }
}
