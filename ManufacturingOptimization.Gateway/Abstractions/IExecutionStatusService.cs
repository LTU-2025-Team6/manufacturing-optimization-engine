using ManufacturingOptimization.Gateway.DTOs;

namespace ManufacturingOptimization.Gateway.Abstractions;

/// <summary>
/// Service for retrieving execution status and monitoring.
/// </summary>
public interface IExecutionStatusService
{
    /// <summary>
    /// Get summary of all execution plans with their progress.
    /// </summary>
    Task<List<ExecutionPlanSummaryDto>> GetAllPlansAsync();

    /// <summary>
    /// Get detailed information about a specific execution plan.
    /// </summary>
    Task<ExecutionPlanDetailDto> GetPlanDetailAsync(Guid id);

    /// <summary>
    /// Get all plans currently in progress.
    /// </summary>
    Task<List<ExecutionPlanSummaryDto>> GetInProgressPlansAsync();

    /// <summary>
    /// Get execution steps for a specific plan.
    /// </summary>
    Task<List<ExecutionStepDto>> GetPlanStepsAsync(Guid id);

    /// <summary>
    /// Get overall execution statistics and summary.
    /// </summary>
    Task<ExecutionSummaryDto> GetExecutionSummaryAsync();
}
