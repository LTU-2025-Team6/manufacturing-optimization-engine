using ManufacturingOptimization.Common.Models.Data.Abstractions;
using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;

namespace ManufacturingOptimization.Engine.Handlers;

public class ProviderStoppedHandler : IMessageHandler<ProviderStoppedEvent>
{
    private readonly IProviderRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProviderStoppedHandler> _logger;

    public ProviderStoppedHandler(
        IProviderRepository repository,
        IMapper mapper,
        ILogger<ProviderStoppedHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task HandleAsync(ProviderStoppedEvent evt)
    {
        var provider = await _repository.GetByIdAsync(evt.ProviderId);

        if (provider == null)
            throw new InvalidOperationException($"Stop event cannot be handled for provider {evt.ProviderId} as it is not registered in engine");

        await _repository.DeleteAsync(provider);
        await _repository.SaveChangesAsync();
    }
}