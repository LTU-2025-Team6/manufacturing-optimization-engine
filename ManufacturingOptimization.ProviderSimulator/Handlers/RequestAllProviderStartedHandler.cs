using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.ProviderSimulator.Abstractions;

namespace ManufacturingOptimization.ProviderSimulator.Handlers;

public sealed class RequestAllProviderStartedHandler : IMessageHandler<RequestAllProviderStartedCommand>
{
    private readonly IProviderSimulationContext _providerContext;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<RequestAllProviderStartedHandler> _logger;

    public RequestAllProviderStartedHandler(
        IProviderSimulationContext providerContext,
        IMessagePublisher messagePublisher,
        ILogger<RequestAllProviderStartedHandler> logger)
    {
        _providerContext = providerContext;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public Task HandleAsync(RequestAllProviderStartedCommand command)
    {
        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.ProviderStarted, new ProviderStartedEvent
        {
            Provider = _providerContext.Provider
        });

        return Task.CompletedTask;
    }
}