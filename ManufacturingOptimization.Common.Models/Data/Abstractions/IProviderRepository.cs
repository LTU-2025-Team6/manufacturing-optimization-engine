using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Common.Models.Data.Abstractions;

public interface IProviderRepository : IRepository<ProviderEntity>
{
    Task<List<(ProviderEntity ProviderEntity, ProcessCapabilityEntity Capability)>> FindByProcess(ProcessType process, CancellationToken cancellationToken = default);
    Task UpdateRunningStateAsync(Guid providerId, bool isRunning, CancellationToken cancellationToken = default);
    Task DeleteAllAsync(CancellationToken cancellationToken = default);
    Task<List<ProviderEntity>> GetRunningProvidersAsync();
    Task<List<ProviderEntity>> GetProvidersWithCapabilityAsync(ProcessType process, Guid? excludedProviderId = null);
}
