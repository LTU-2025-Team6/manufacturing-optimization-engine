using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.Gateway.Abstractions;

public interface IProviderOrchestrator
{
    Task StartAsync(ProviderEntity provider, CancellationToken cancellationToken = default);
    Task StopAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task CleanupOrphanedContainersAsync(CancellationToken cancellationToken = default);
}
