using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Settings;
using Microsoft.Extensions.Options;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;

namespace ManufacturingOptimization.Gateway.Handlers;

public class StopProviderHandler : IMessageHandler<StopProviderCommand>
{
    private readonly OrchestrationSettings _orchestrationSettings;
    private readonly IProviderOrchestrator _providerOrchestrator;
    private readonly IProviderRepository _providerRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly INotificationPublisher _notificationPublisher;

    public StopProviderHandler(
        IOptions<OrchestrationSettings> orchestrationSettings,
        IProviderOrchestrator providerOrchestrator,
        IProviderRepository providerRepository,
        IMessagePublisher messagePublisher,
        INotificationPublisher notificationPublisher)
    {
        _orchestrationSettings = orchestrationSettings.Value;
        _providerOrchestrator = providerOrchestrator;
        _providerRepository = providerRepository;
        _messagePublisher = messagePublisher;
        _notificationPublisher = notificationPublisher;
    }

    public async Task HandleAsync(StopProviderCommand evt)
    {
        _notificationPublisher.NotifyStoppingProvider(evt.ProviderId);

        var provider = await _providerRepository.GetByIdAsync(evt.ProviderId);

        if (provider == null)
            throw new InvalidOperationException($"Provider with Id {evt.ProviderId} not found in gateway.");

        if (!provider.IsRunning)
            throw new InvalidOperationException($"Provider with Id {evt.ProviderId} is not running.");

        if (_orchestrationSettings.IsProductionMode)
        {
            await _providerOrchestrator.StopAsync(evt.ProviderId);
        }

        provider.IsRunning = false;
        await _providerRepository.UpdateAsync(provider);
        await _providerRepository.SaveChangesAsync();

        _messagePublisher.Publish(
            Exchanges.Provider,
            ProviderRoutingKeys.ProviderStopped,
            new ProviderStoppedEvent { ProviderId = provider.Id });
    }
}
