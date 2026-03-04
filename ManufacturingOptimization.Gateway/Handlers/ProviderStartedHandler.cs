using AutoMapper;
using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Common.Contracts;
using ManufacturingOptimization.Gateway.Data.Entities;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Settings;
using Microsoft.Extensions.Options;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;

namespace ManufacturingOptimization.Gateway.Handlers;

public class ProviderStartedHandler : IMessageHandler<ProviderStartedEvent>
{
    private readonly IMapper _mapper;
    private readonly OrchestrationSettings _orchestrationSettings;
    private readonly IProviderRepository _providerRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly INotificationPublisher _notificationPublisher;

    public ProviderStartedHandler(
        IMapper mapper,
        IOptions<OrchestrationSettings> orchestrationSettings,
        IProviderRepository providerRepository,
        IMessagePublisher messagePublisher,
        INotificationPublisher notificationPublisher)
    {
        _mapper = mapper;
        _orchestrationSettings = orchestrationSettings.Value;
        _providerRepository = providerRepository;
        _messagePublisher = messagePublisher;
        _notificationPublisher = notificationPublisher;
    }

    public Task HandleAsync(ProviderStartedEvent evt)
    {
        _notificationPublisher.NotifyProviderStarted(evt.Provider.Name);

        //if (_orchestrationSettings.IsDevelopmentMode)
        //{
        //    var providerEntity = _mapper.Map<ProviderEntity>(evt.Provider);
        //    providerEntity.IsRunning = true;

        //    await _providerRepository.AddAsync(providerEntity);
        //    await _providerRepository.SaveChangesAsync();
        //}

        //if (_orchestrationSettings.IsProductionMode)
        //{
        //    await _providerRepository.UpdateRunningStateAsync(evt.Provider.Id, true);
        //}
        return Task.CompletedTask;
    }
}
