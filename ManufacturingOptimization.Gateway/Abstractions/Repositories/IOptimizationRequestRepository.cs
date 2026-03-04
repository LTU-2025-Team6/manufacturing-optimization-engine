using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Gateway.Data.Entities;

namespace ManufacturingOptimization.Gateway.Abstractions.Repositories;

public interface IOptimizationRequestRepository : IRepository<OptimizationRequestEntity>
{
    Task<OptimizationRequestEntity?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<OptimizationRequestEntity>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);
}
