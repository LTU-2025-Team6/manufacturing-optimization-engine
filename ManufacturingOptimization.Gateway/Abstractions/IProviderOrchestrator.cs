using ManufacturingOptimization.Gateway.Data.Entities;

namespace ManufacturingOptimization.Gateway.Abstractions;

public interface IProviderOrchestrator
{
    Task StartAsync(ProviderEntity provider, CancellationToken cancellationToken = default);
    Task StopAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task CleanupOrphanedContainersAsync(CancellationToken cancellationToken = default);
}
