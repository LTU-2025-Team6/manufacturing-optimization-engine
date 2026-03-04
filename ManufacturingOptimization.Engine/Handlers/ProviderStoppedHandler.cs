using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Engine.Abstractions;

namespace ManufacturingOptimization.Engine.Handlers;

public class ProviderStoppedHandler : IMessageHandler<ProviderStoppedEvent>
{
    private readonly IProviderRepository _repository;
    private readonly ILogger<ProviderStoppedHandler> _logger;

    public ProviderStoppedHandler(
        IProviderRepository repository,
        ILogger<ProviderStoppedHandler> logger)
    {
        _repository = repository;
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