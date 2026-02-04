using ManufacturingOptimization.Common.Models.Data.Abstractions;
using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.Engine.Handlers;

public class ProviderUpdatedHandler : IMessageHandler<ProviderUpdatedEvent>
{
    private readonly IProviderRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProviderUpdatedHandler> _logger;

    public ProviderUpdatedHandler(
        IProviderRepository repository,
        IMapper mapper,
        ILogger<ProviderUpdatedHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task HandleAsync(ProviderUpdatedEvent evt)
    {
        var provider = await _repository.GetByIdAsync(evt.Provider.Id);

        if (provider == null)
            throw new InvalidOperationException($"Stop event cannot be handled for provider {evt.Provider.Id} as it is not registered in engine");

        await _repository.DeleteAsync(provider);
        await _repository.AddAsync(_mapper.Map<ProviderEntity>(evt.Provider));
        await _repository.SaveChangesAsync();
    }
}