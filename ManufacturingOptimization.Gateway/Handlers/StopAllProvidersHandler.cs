using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Settings;
using Microsoft.Extensions.Options;

namespace ManufacturingOptimization.Gateway.Handlers;

public class StopAllProvidersHandler : IMessageHandler<StopAllProvidersCommand>
{
    private readonly OrchestrationSettings _orchestrationSettings;
    private readonly IProviderOrchestrator _providerOrchestrator;
    private readonly IProviderRepository _providerRepository;
    private readonly IMessagePublisher _messagePublisher;

    public StopAllProvidersHandler(
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

    public async Task HandleAsync(StopAllProvidersCommand evt)
    {
        if (_orchestrationSettings.IsDevelopmentMode)
            return;

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
