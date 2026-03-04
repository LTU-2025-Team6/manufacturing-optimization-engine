using AutoMapper;
using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Engine.Abstractions;

namespace ManufacturingOptimization.Engine.Handlers;

public class ProviderUpdatedHandler : IMessageHandler<ProviderUpdatedEvent>
{
    private readonly IProviderRepository _repository;
    private readonly ILogger<ProviderUpdatedHandler> _logger;

    public ProviderUpdatedHandler(
        IProviderRepository repository,
        ILogger<ProviderUpdatedHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(ProviderUpdatedEvent evt)
    {
        var provider = await _repository.GetByIdAsync(evt.Provider.Id);

        if (provider == null)
            throw new InvalidOperationException($"Stop event cannot be handled for provider {evt.Provider.Id} as it is not registered in engine");

        await _repository.DeleteAsync(provider);
        await _repository.AddAsync(evt.Provider);
        await _repository.SaveChangesAsync();
    }
}