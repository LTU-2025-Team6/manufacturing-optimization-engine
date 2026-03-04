using ManufacturingOptimization.Gateway.DTOs.Common;
using ManufacturingOptimization.Gateway.DTOs.Execution;

namespace ManufacturingOptimization.Gateway.Abstractions.Services;

/// <summary>
/// Service for retrieving execution status and monitoring.
/// </summary>
public interface IExecutionStatusService
{
    /// <summary>
    /// Get summary of all execution plans with their progress.
    /// </summary>
    Task<PagedResult<ExecutionPlanSummaryDto>> GetAllPlansAsync(PaginationRequest pagination);

    /// <summary>
    /// Get detailed information about a specific execution plan.
    /// </summary>
    Task<ExecutionPlanDetailDto> GetPlanDetailAsync(Guid id);

    /// <summary>
    /// Get all plans currently in progress.
    /// </summary>
    Task<PagedResult<ExecutionPlanSummaryDto>> GetInProgressPlansAsync(PaginationRequest pagination);

    /// <summary>
    /// Get execution steps for a specific plan.
    /// </summary>
    Task<List<ExecutionStepDto>> GetPlanStepsAsync(Guid id);

    /// <summary>
    /// Get overall execution statistics and summary.
    /// </summary>
    Task<ExecutionSummaryDto> GetExecutionSummaryAsync();
}
