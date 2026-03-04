using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Gateway.Data.Entities;

namespace ManufacturingOptimization.Gateway.Abstractions.Repositories;

/// <summary>
/// Repository for managing optimization strategy entities.
/// </summary>
public interface IOptimizationStrategyRepository : IRepository<OptimizationStrategyEntity>
{
    /// <summary>
    /// Gets strategy by ID with only Steps loaded (without Estimate, ProviderSchedule details).
    /// Use for: GetAlternativeProviders, CancelPlanAsync when only Step IDs and basic info needed.
    /// </summary>
    Task<OptimizationStrategyEntity?> GetByIdWithStepsOnlyAsync(Guid id, CancellationToken cancellationToken = default);
}
