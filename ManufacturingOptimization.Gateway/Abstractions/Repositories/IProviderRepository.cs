using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Enums;
using ManufacturingOptimization.Gateway.Data.Entities;

namespace ManufacturingOptimization.Gateway.Abstractions.Repositories;

public interface IProviderRepository : IRepository<ProviderEntity>
{
    /// <summary>
    /// Gets provider by ID with all related data (ProcessCapabilities, TechnicalCapabilities, WorkingHours+Breaks).
    /// Use for: GetProviderByIdAsync (API), UpdateProviderAsync, StartProviderHandler (Docker orchestrator)
    /// </summary>
    Task<ProviderEntity?> GetByIdWithFullDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ProviderEntity>> GetAllWithFullDetailsAsync(CancellationToken cancellationToken = default);
    Task<(List<ProviderEntity> items, int totalCount)> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);

    Task UpdateRunningStateAsync(Guid providerId, bool isRunning, CancellationToken cancellationToken = default);
    Task DeleteAllAsync(CancellationToken cancellationToken = default);
    Task<List<ProviderEntity>> GetRunningProvidersAsync();
    Task<List<ProviderEntity>> GetProvidersWithCapabilityAsync(ProcessType process, Guid? excludedProviderId = null);
    Task<int> GetActiveProvidersCountAsync(CancellationToken cancellationToken = default);
}
