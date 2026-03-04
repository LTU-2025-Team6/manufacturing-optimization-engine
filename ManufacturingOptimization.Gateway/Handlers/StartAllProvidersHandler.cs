using AutoMapper;
using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Contracts;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;
using ManufacturingOptimization.Gateway.Data.Entities;
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
    private readonly INotificationPublisher _notificationPublisher;
    private readonly IAsyncAwaiter _asyncAwaiter;

    public StartAllProvidersHandler(
        IMapper mapper,
        IOptions<OrchestrationSettings> orchestrationSettings,
        IProviderOrchestrator providerOrchestrator,
        IProviderRepository providerRepository,
        IMessagePublisher messagePublisher,
        INotificationPublisher notificationPublisher,
        IAsyncAwaiter asyncAwaiter)
    {
        _mapper = mapper;
        _orchestrationSettings = orchestrationSettings.Value;
        _providerOrchestrator = providerOrchestrator;
        _providerRepository = providerRepository;
        _messagePublisher = messagePublisher;
        _notificationPublisher = notificationPublisher;
        _asyncAwaiter = asyncAwaiter;
    }

    public async Task HandleAsync(StartAllProvidersCommand evt)
    {
        // Notify request received
        _notificationPublisher.NotifyStartingAllProviders();

        await _providerOrchestrator.CleanupOrphanedContainersAsync();

        IEnumerable<ProviderEntity>? providers = null;
        if (_orchestrationSettings.IsProductionMode)
        {
            providers = (await _providerRepository.GetAllWithFullDetailsAsync())
                .Where(p => p.AutoStart)
                .ToList();

            // Start providers directly
            var errors = new List<string>();
            foreach (var provider in providers)
            {
                try
                {
                    await _providerOrchestrator.StartAsync(provider);
                    provider.IsRunning = true;
                }
                catch (Exception ex)
                {
                    errors.Add($"Provider {provider.Name} start failed: {ex.Message}");
                }
            }

            if (errors.Any())
                throw new Exception("One or more providers failed to start.", new AggregateException(errors.Select(e => new Exception(e))));

            await _providerRepository.SaveChangesAsync();

            // Wait for RabbitMQ setup in containers
            await Task.Delay(3000);
        }

        // Request providers registration
        var runningProviders = await TriggerAndAwaitRegistrationStartAsync(providers);

        if (_orchestrationSettings.IsDevelopmentMode)
        {
            foreach (var providerModel in runningProviders)
            {
                var providerEntity = _mapper.Map<ProviderEntity>(providerModel);
                providerEntity.IsRunning = true;
                await _providerRepository.AddAsync(providerEntity);
            }
            
            await _providerRepository.SaveChangesAsync();
        }

        _messagePublisher.Publish(
            Exchanges.Provider,
            ProviderRoutingKeys.AllProvidersStarted,
            new AllProvidersStartedEvent());
        
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
