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

public class StartAllProvidersHandler : IMessageHandler<StartAllProvidersCommand>
{
    private const int ExpectedProviderCountInDevelopment = 3;

    private readonly IMapper _mapper;
    private readonly OrchestrationSettings _orchestrationSettings;
    private readonly IProviderOrchestrator _providerOrchestrator;
    private readonly IProviderRepository _providerRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IAsyncAwaiter _asyncAwaiter;

    public StartAllProvidersHandler(
        IMapper mapper,
        IOptions<OrchestrationSettings> orchestrationSettings,
        IProviderOrchestrator providerOrchestrator,
        IProviderRepository providerRepository,
        IMessagePublisher messagePublisher,
        IAsyncAwaiter asyncAwaiter)
    {
        _mapper = mapper;
        _orchestrationSettings = orchestrationSettings.Value;
        _providerOrchestrator = providerOrchestrator;
        _providerRepository = providerRepository;
        _messagePublisher = messagePublisher;
        _asyncAwaiter = asyncAwaiter;
    }

    public async Task HandleAsync(StartAllProvidersCommand evt)
    {
        await _providerOrchestrator.CleanupOrphanedContainersAsync();

        IEnumerable<ProviderEntity>? providers = null;
        if (_orchestrationSettings.IsProductionMode)
        {
            providers = (await _providerRepository.GetAllAsync())
                .Where(p => p.AutoStart)
                .ToList();

            var errors = new List<string>();
            var containerStartTasks = providers.Select(p => TriggerAndAwaitContainerStartAsync(p, errors));
            await Task.WhenAll(containerStartTasks);

            if (errors.Any())
                throw new Exception("One or more providers failed to start. See inner exception for details.", new AggregateException(errors.Select(e => new Exception(e))));

            // Time for containers to setup RabbitMq
            await Task.Delay(3000);
        }

        // Request providers registration
        var runningProviders = await TriggerAndAwaitRegistrationStartAsync(providers);

        foreach (var provider in runningProviders)
        {
            provider.IsRunning = true;

            if (_orchestrationSettings.IsDevelopmentMode)
            {
                var providerEntity = _mapper.Map<ProviderEntity>(provider);
                await _providerRepository.AddAsync(providerEntity);
            }

            if (_orchestrationSettings.IsProductionMode)
            {
                var providerEntity = providers?.FirstOrDefault(p => p.Id == provider.Id);
                if (providerEntity != null)
                    providerEntity.IsRunning = true;
            }
        }

        await _providerRepository.SaveChangesAsync();

        _messagePublisher.Publish(
            Exchanges.Provider,
            ProviderRoutingKeys.AllProvidersStarted,
            new AllProvidersStartedEvent());
        
    }

    private async Task TriggerAndAwaitContainerStartAsync(ProviderEntity provider, List<string> errors)
    {
        try
        {
            var response = await _asyncAwaiter.AwaitAsync(new AwaitScenario<ProviderContainerStartedEvent>
            {
                Exchange = Exchanges.Provider,
                RoutingKey = ProviderRoutingKeys.ProviderContainerStarted,
                Match = evt => evt.ProviderId == provider.Id,
                Timeout = TimeSpan.FromSeconds(10),
                BeforeAwait = () => _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.StartProvider, new StartProviderCommand
                {
                    ProviderId = provider.Id
                })
            });
        }
        catch (Exception ex)
        {
            errors.Add($"Provider {provider.Name} start failed: {ex.Message}");
        }
    }

    private async Task<IEnumerable<ProviderModel>> TriggerAndAwaitRegistrationStartAsync(IEnumerable<ProviderEntity>? providers = null)
    {
        var expectedProviderIds = providers?.Select(p => p.Id).ToHashSet();

        var response = await _asyncAwaiter.AwaitMultipleAsync(new AwaitMultipleScenario<ProviderStartedEvent>
        {
            Exchange = Exchanges.Provider,
            RoutingKey = ProviderRoutingKeys.ProviderStarted,
            CompletionCondition = responses =>
            {
                if (_orchestrationSettings.IsProductionMode && expectedProviderIds != null)
                    return responses.Select(r => r.Provider.Id)
                        .ToHashSet()
                        .SetEquals(expectedProviderIds);

                if (_orchestrationSettings.IsDevelopmentMode)
                    return responses.Count == ExpectedProviderCountInDevelopment;

                return false;
            },
            Match = evt => expectedProviderIds?.Contains(evt.Provider.Id) ?? true,
            Timeout = TimeSpan.FromSeconds(20),
            BeforeAwait = () => _messagePublisher.Publish(
                Exchanges.Provider,
                ProviderRoutingKeys.RequestAllProviderStarted,
                new RequestAllProviderStartedCommand())
        });

        return response.Select(e => e.Provider);
    }
}
