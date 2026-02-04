using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Settings;
using Microsoft.Extensions.Options;

namespace ManufacturingOptimization.Gateway.Handlers;

public class StartAllProvidersHandler : IMessageHandler<StartAllProvidersCommand>
{
    private readonly IMapper _mapper;
    private readonly OrchestrationSettings _orchestrationSettings;
    private readonly IProviderOrchestrator _providerOrchestrator;
    private readonly IProviderRepository _providerRepository;
    private readonly IMessagePublisher _messagePublisher;

    public StartAllProvidersHandler(
        IMapper mapper,
        IOptions<OrchestrationSettings> orchestrationSettings,
        IProviderOrchestrator providerOrchestrator,
        IProviderRepository providerRepository,
        IMessagePublisher messagePublisher)
    {
        _mapper = mapper;
        _orchestrationSettings = orchestrationSettings.Value;
        _providerOrchestrator = providerOrchestrator;
        _providerRepository = providerRepository;
        _messagePublisher = messagePublisher;
    }

    public async Task HandleAsync(StartAllProvidersCommand evt)
    {
        await _providerOrchestrator.CleanupOrphanedContainersAsync();

        if (_orchestrationSettings.IsDevelopmentMode)
            return;

        var providers = await _providerRepository.GetAllAsync();
        foreach (var provider in providers)
        {
            if (!provider.AutoStart)
                continue;

            _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.StartProvider, new StartProviderCommand
            {
                ProviderId = provider.Id
            });
        }
    }
}
