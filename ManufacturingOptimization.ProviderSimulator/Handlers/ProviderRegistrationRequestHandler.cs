using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.ProviderSimulator.Abstractions;

namespace ManufacturingOptimization.ProviderSimulator.Handlers;

/// <summary>
/// Handles provider registration requests by publishing provider information.
/// </summary>
public class ProviderRegistrationRequestHandler : IMessageHandler<RequestProvidersRegistrationCommand>
{
    private readonly IProviderSimulationContext _providerLogic;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<ProviderRegistrationRequestHandler> _logger;

    public ProviderRegistrationRequestHandler(
        IProviderSimulationContext providerLogic,
        IMessagePublisher messagePublisher,
        ILogger<ProviderRegistrationRequestHandler> logger)
    {
        _providerLogic = providerLogic;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public Task HandleAsync(RequestProvidersRegistrationCommand command)
    {
        var registeredEvent = new ProviderRegisteredEvent
        {
            Provider = _providerLogic.Provider
        };

        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.Registered, registeredEvent);
            
        return Task.CompletedTask;
    }
}
