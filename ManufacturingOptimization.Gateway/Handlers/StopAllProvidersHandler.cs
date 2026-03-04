using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Settings;
using Microsoft.Extensions.Options;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;

namespace ManufacturingOptimization.Gateway.Handlers;

public class StopAllProvidersHandler : IMessageHandler<StopAllProvidersCommand>
{
    private readonly OrchestrationSettings _orchestrationSettings;
    private readonly IProviderOrchestrator _providerOrchestrator;
    private readonly IProviderRepository _providerRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly INotificationPublisher _notificationPublisher;

    public StopAllProvidersHandler(
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

    public async Task HandleAsync(StopAllProvidersCommand evt)
    {
        if (_orchestrationSettings.IsDevelopmentMode)
            return;

        _notificationPublisher.NotifyStoppingAllProviders();

        var runningProvider = await _providerRepository.GetRunningProvidersAsync();
        foreach (var provider in runningProvider)
        {
            await _providerOrchestrator.StopAsync(provider.Id);

            _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.ProviderStopped, new ProviderStoppedEvent
            {
                ProviderId = provider.Id
            });
        }

        await Task.Delay(3000); // Wait a moment to ensure all stop commands are processed
        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.AllProvidersStopped, new AllProvidersStoppedEvent());
    }
}
