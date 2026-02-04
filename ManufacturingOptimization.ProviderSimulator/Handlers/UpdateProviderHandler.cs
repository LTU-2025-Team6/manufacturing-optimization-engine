using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.ProviderSimulator.Abstractions;

namespace ManufacturingOptimization.ProviderSimulator.Handlers;

public sealed class UpdateProviderHandler : IMessageHandler<UpdateProviderCommand>
{
    private readonly IProviderSimulationContext _providerContext;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<ProcessConfirmationHandler> _logger;

    public UpdateProviderHandler(
        IProviderSimulationContext providerContext,
        IMessagePublisher messagePublisher,
        ILogger<ProcessConfirmationHandler> logger)
    {
        _providerContext = providerContext;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public Task HandleAsync(UpdateProviderCommand command)
    {
        if (command.Provider.Id != _providerContext.Provider.Id)
            return Task.CompletedTask;

        _providerContext.Provider = command.Provider;
        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.ProviderUpdated, new ProviderUpdatedEvent
        {
            Provider = _providerContext.Provider
        });

        return Task.CompletedTask;
    }
}
