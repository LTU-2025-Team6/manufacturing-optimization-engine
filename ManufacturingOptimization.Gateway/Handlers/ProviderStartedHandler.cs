using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Settings;
using Microsoft.Extensions.Options;

namespace ManufacturingOptimization.Gateway.Handlers;

public class ProviderStartedHandler : IMessageHandler<ProviderStartedEvent>
{
    private const int ExpectedProviderCountInDevelopment = 3;

    private readonly IMapper _mapper;
    private readonly OrchestrationSettings _orchestrationSettings;
    private readonly IProviderRepository _providerRepository;
    private readonly IMessagePublisher _messagePublisher;

    public ProviderStartedHandler(
        IMapper mapper,
        IOptions<OrchestrationSettings> orchestrationSettings,
        IProviderRepository providerRepository,
        IMessagePublisher messagePublisher)
    {
        _mapper = mapper;
        _orchestrationSettings = orchestrationSettings.Value;
        _providerRepository = providerRepository;
        _messagePublisher = messagePublisher;
    }

    public async Task HandleAsync(ProviderStartedEvent evt)
    {
        var allRunning = false;

        if (_orchestrationSettings.IsDevelopmentMode)
        {
            // DatabaseManagmentService clears the database on each start in development mode
            // so we need to add the provider each time it registers

            var providerEntity = _mapper.Map<ProviderEntity>(evt.Provider);
            providerEntity.IsRunning = true;

            await _providerRepository.AddAsync(providerEntity);
            await _providerRepository.SaveChangesAsync();

            allRunning = _providerRepository.Count == ExpectedProviderCountInDevelopment;
        }
        else if (_orchestrationSettings.IsProductionMode)
        {
            await _providerRepository.UpdateRunningState(evt.Provider.Id, true);
            allRunning = await _providerRepository.AreAllRunning();
        }

        if (allRunning)
        {
            var runningProviders = await _providerRepository.GetRunningProvidersAsync();

            await Task.Delay(3000); // Wait a moment to ensure all registrations are processed
            _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.AllProvidersStarted, new AllProvidersStartedEvent
            {
                RunningProviders = _mapper.Map<List<ProviderModel>>(runningProviders)
            });
        }
    }
}
