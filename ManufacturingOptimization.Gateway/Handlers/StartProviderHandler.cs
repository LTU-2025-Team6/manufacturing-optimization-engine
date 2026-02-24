using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Settings;
using Microsoft.Extensions.Options;

namespace ManufacturingOptimization.Gateway.Handlers;

public class StartProviderHandler : IMessageHandler<StartProviderCommand>
{
    private readonly OrchestrationSettings _orchestrationSettings;
    private readonly IProviderOrchestrator _providerOrchestrator;
    private readonly IProviderRepository _providerRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly INotificationPublisher _notificationPublisher;

    public StartProviderHandler(
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

    public async Task HandleAsync(StartProviderCommand evt)
    {
        if (_orchestrationSettings.IsDevelopmentMode)
            return;

        _notificationPublisher.NotifyStartingProvider(evt.ProviderId);

        var provider = await _providerRepository.GetByIdAsync(evt.ProviderId);

        if (provider == null)
            throw new InvalidOperationException($"Provider with Id {evt.ProviderId} not found in gateway.");

        if (provider.IsRunning)
            throw new InvalidOperationException($"Provider with Id {evt.ProviderId} is already running.");

        await _providerOrchestrator.StartAsync(provider);

        _messagePublisher.Publish(
            Exchanges.Provider,
            ProviderRoutingKeys.ProviderContainerStarted,
            new ProviderContainerStartedEvent { ProviderId = provider.Id });
    }
}
