using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Gateway.Settings;
using Microsoft.Extensions.Options;

public class ProviderRegisteredHandler : IMessageHandler<ProviderRegisteredEvent>
{
    private const int ExpectedProviderCountInDevelopment = 3;

    private readonly ILogger<ProviderRegisteredHandler> _logger;
    private readonly IMapper _mapper;
    private readonly OrchestrationSettings _orchestrationSettings;
    private readonly IProviderRepository _providerRepository;
    private readonly IMessagePublisher _messagePublisher;

    public ProviderRegisteredHandler(
        ILogger<ProviderRegisteredHandler> logger,
        IMapper mapper,
        IOptions<OrchestrationSettings> orchestrationSettings,
        IMessagePublisher messagePublisher,
        IProviderRepository providerRepository)
    {
        _logger = logger;
        _mapper = mapper;
        _orchestrationSettings = orchestrationSettings.Value;
        _providerRepository = providerRepository;
        _messagePublisher = messagePublisher;
    }

    public async Task HandleAsync(ProviderRegisteredEvent evt)
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
            await Task.Delay(2000); // Wait a moment to ensure all registrations are processed
            _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.AllRegistered, new AllProvidersRegisteredEvent());
        }
    }

}
