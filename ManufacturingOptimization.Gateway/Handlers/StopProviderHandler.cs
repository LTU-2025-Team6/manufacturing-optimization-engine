using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Settings;
using Microsoft.Extensions.Options;

namespace ManufacturingOptimization.Gateway.Handlers;

public class StopProviderHandler : IMessageHandler<StopProviderCommand>
{
    private readonly OrchestrationSettings _orchestrationSettings;
    private readonly IProviderOrchestrator _providerOrchestrator;
    private readonly IProviderRepository _providerRepository;
    private readonly IMessagePublisher _messagePublisher;

    public StopProviderHandler(
        IOptions<OrchestrationSettings> orchestrationSettings,
        IProviderOrchestrator providerOrchestrator,
        IProviderRepository providerRepository,
        IMessagePublisher messagePublisher)
    {
        _orchestrationSettings = orchestrationSettings.Value;
        _providerOrchestrator = providerOrchestrator;
        _providerRepository = providerRepository;
        _messagePublisher = messagePublisher;
    }

    public async Task HandleAsync(StopProviderCommand evt)
    {
        if (_orchestrationSettings.IsDevelopmentMode)
            return;

        var provider = await _providerRepository.GetByIdAsync(evt.ProviderId);

        if (provider == null)
            throw new InvalidOperationException($"Provider with Id {evt.ProviderId} not found in gateway.");

        if (!provider.IsRunning)
            throw new InvalidOperationException($"Provider with Id {evt.ProviderId} is not running.");

        await _providerOrchestrator.StopAsync(evt.ProviderId);

        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.ProviderStopped, new ProviderStoppedEvent
        {
            ProviderId = provider.Id
        });
    }
}
