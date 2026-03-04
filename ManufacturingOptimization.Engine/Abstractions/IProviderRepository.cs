using ManufacturingOptimization.Common.Contracts;
using ManufacturingOptimization.Common.Enums;

namespace ManufacturingOptimization.Engine.Abstractions;

public interface IProviderRepository
{
    Task<ProviderModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProviderModel> AddAsync(ProviderModel entity, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProviderModel>> AddRangeAsync(IEnumerable<ProviderModel> entities, CancellationToken cancellationToken = default);
    Task DeleteAsync(ProviderModel entity, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<List<(ProviderModel Provider, ProcessCapabilityModel Capability)>> FindByProcess(ProcessType process, CancellationToken cancellationToken = default);
}