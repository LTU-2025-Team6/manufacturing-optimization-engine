using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.Gateway.Abstractions;

/// <summary>
/// Provider orchestration interface.
/// Production mode: creates containers via Docker API.
/// Development mode: no-op, providers managed by docker-compose.
/// </summary>
public interface IProviderOrchestrator
{
    Task StartAllAsync(CancellationToken cancellationToken = default);
    Task StopAllAsync(CancellationToken cancellationToken = default);
    Task StartAsync(ProviderEntity provider, CancellationToken cancellationToken = default);
    Task StopAsync(Guid providerId, CancellationToken cancellationToken = default);
    Task CleanupOrphanedContainersAsync(CancellationToken cancellationToken = default);
}