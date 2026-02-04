using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Engine.Abstractions;
using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;

namespace ManufacturingOptimization.Engine.Handlers;

/// <summary>
/// Handles provider registration events by persisting provider data to the database.
/// Registered as Scoped service - dependencies can be injected directly.
/// </summary>
public class ProviderStartedHandler : IMessageHandler<ProviderStartedEvent>
{
    private readonly IProviderRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProviderStartedHandler> _logger;
    public ProviderStartedHandler(
        IProviderRepository repository,
        IMapper mapper,
        ILogger<ProviderStartedHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task HandleAsync(ProviderStartedEvent evt)
    {
        var providerEntity = _mapper.Map<ProviderEntity>(evt.Provider);
        await _repository.AddAsync(providerEntity);
        await _repository.SaveChangesAsync();
    }
}
