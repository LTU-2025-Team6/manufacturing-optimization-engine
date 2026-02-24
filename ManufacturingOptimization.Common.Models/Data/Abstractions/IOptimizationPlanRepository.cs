using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.Common.Models.Data.Abstractions;

/// <summary>
/// Repository for storing and retrieving optimization plans.
/// </summary>
public interface IOptimizationPlanRepository : IRepository<OptimizationPlanEntity>
{
    Task<OptimizationPlanEntity?> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetRunningCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetCompletedThisMonthCountAsync(CancellationToken cancellationToken = default);
}
