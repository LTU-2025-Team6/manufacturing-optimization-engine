using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Common.Models.Data.Abstractions;

public interface IProviderRepository : IRepository<ProviderEntity>
{
    Task<List<(ProviderEntity ProviderEntity, ProcessCapabilityEntity Capability)>> FindByProcess(ProcessType process, CancellationToken cancellationToken = default);
    Task UpdateRunningState(Guid providerId, bool isRunning, CancellationToken cancellationToken = default);
    Task UpdateAllRunningState(bool isRunning, CancellationToken cancellationToken = default);
    Task<bool> AreAllRunning(CancellationToken cancellationToken = default);
    Task DeleteAllAsync(CancellationToken cancellationToken = default);
}
