using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Settings;
using Microsoft.Extensions.Options;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;

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
        var provider = await _providerRepository.GetByIdWithFullDetailsAsync(evt.ProviderId);

        if (provider == null)
            throw new InvalidOperationException($"Provider with Id {evt.ProviderId} not found in gateway.");

        if (provider.IsRunning)
            throw new InvalidOperationException($"Provider with Id {evt.ProviderId} is already running.");

        if (_orchestrationSettings.IsProductionMode)
        {
            await _providerOrchestrator.StartAsync(provider);
        }

        // Update status in DB in both modes
        provider.IsRunning = true;
        await _providerRepository.UpdateAsync(provider);
        await _providerRepository.SaveChangesAsync();
    }
}
