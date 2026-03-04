using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Settings;
using Microsoft.Extensions.Options;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;

namespace ManufacturingOptimization.Gateway.Handlers;

public class ProviderStoppedHandler : IMessageHandler<ProviderStoppedEvent>
{
    private readonly OrchestrationSettings _orchestrationSettings;
    private readonly IProviderOrchestrator _providerOrchestrator;
    private readonly IProviderRepository _providerRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly INotificationPublisher _notificationPublisher;

    public ProviderStoppedHandler(
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

    public async Task HandleAsync(ProviderStoppedEvent evt)
    {
        await _providerRepository.UpdateRunningStateAsync(evt.ProviderId, false);
        _notificationPublisher.NotifyProviderStopped(evt.ProviderId);
    }
}