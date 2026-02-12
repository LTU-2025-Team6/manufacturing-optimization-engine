using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.Common.Models.Data.Abstractions;

public interface IOptimizationRequestRepository : IRepository<OptimizationRequestEntity>
{
    Task<OptimizationRequestEntity?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<OptimizationRequestEntity>> GetByCustomerIdAsync(string customerId, CancellationToken cancellationToken = default);
}
