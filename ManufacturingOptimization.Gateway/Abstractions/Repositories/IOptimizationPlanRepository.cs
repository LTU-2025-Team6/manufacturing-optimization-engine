using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Gateway.Data.Entities;

namespace ManufacturingOptimization.Gateway.Abstractions.Repositories;

/// <summary>
/// Repository for storing and retrieving optimization plans.
/// </summary>
public interface IOptimizationPlanRepository : IRepository<OptimizationPlanEntity>
{
    // Specialized query methods
    /// <summary>
    /// Get plan with SelectedStrategy and Steps only - for execution event handlers.
    /// </summary>
    Task<OptimizationPlanEntity?> GetWithSelectedStrategyStepsForExecutionAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get plan with all Strategies and Steps - for plan update handler.
    /// </summary>
    Task<OptimizationPlanEntity?> GetWithAllStrategiesForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get plan by request ID with full details (all strategies, steps, estimates, schedules, metrics, warranties) - for API responses.
    /// </summary>
    Task<OptimizationPlanEntity?> GetByIdWithFullDetailsAsync(Guid requestId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get plan with SelectedStrategy.Steps and ProviderSchedule.Segments - for execution status detail views.
    /// </summary>
    Task<OptimizationPlanEntity?> GetWithSelectedStrategyDetailsForExecutionStatusAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all plans with SelectedStrategy and Steps only - for execution status summaries.
    /// </summary>
    Task<IEnumerable<OptimizationPlanEntity>> GetAllWithSelectedStrategyStepsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get paginated plans ordered by CreatedAt descending.
    /// </summary>
    Task<(List<OptimizationPlanEntity> items, int totalCount)> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get paginated plans with SelectedStrategy and Steps - for execution status summaries.
    /// </summary>
    Task<(List<OptimizationPlanEntity> items, int totalCount)> GetPagedWithSelectedStrategyStepsAsync(int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get paginated plans that have at least one step in progress.
    /// </summary>
    Task<(List<OptimizationPlanEntity> items, int totalCount)> GetPagedInProgressAsync(int skip, int take, CancellationToken cancellationToken = default);
    
    // Statistics
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetRunningCountAsync(CancellationToken cancellationToken = default);
    Task<int> GetCompletedThisMonthCountAsync(CancellationToken cancellationToken = default);
}
