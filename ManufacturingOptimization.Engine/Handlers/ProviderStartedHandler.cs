using AutoMapper;
using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Engine.Abstractions;

namespace ManufacturingOptimization.Engine.Handlers;

/// <summary>
/// Handles provider registration events by persisting provider data to the database.
/// Registered as Scoped service - dependencies can be injected directly.
/// </summary>
public class ProviderStartedHandler : IMessageHandler<ProviderStartedEvent>
{
    private readonly IProviderRepository _repository;
    private readonly ILogger<ProviderStartedHandler> _logger;
    public ProviderStartedHandler(
        IProviderRepository repository,
        ILogger<ProviderStartedHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task HandleAsync(ProviderStartedEvent evt)
    {
        await _repository.AddAsync(evt.Provider);
        await _repository.SaveChangesAsync();
    }
}
