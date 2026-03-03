using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Gateway.DTOs;

/// <summary>
/// DTO for execution plan summary (list view).
/// </summary>
public record ExecutionPlanSummaryDto
{
    public Guid Id { get; init; }
    public Guid RequestId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? ConfirmedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string? ErrorMessage { get; init; }
    
    public int TotalSteps { get; init; }
    public int CompletedSteps { get; init; }
    public int InProgressSteps { get; init; }
    public int FailedSteps { get; init; }
    public int CancelledSteps { get; init; }
    public int PendingSteps { get; init; }
    
    public double ProgressPercentage => TotalSteps > 0 
        ? Math.Round((double)CompletedSteps / TotalSteps * 100, 2) 
        : 0;
}

/// <summary>
/// DTO for detailed execution plan information.
/// </summary>
public record ExecutionPlanDetailDto
{
    public Guid Id { get; init; }
    public Guid RequestId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? ConfirmedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string? ErrorMessage { get; init; }
    
    public List<ExecutionStepDto> Steps { get; init; } = [];
    
    public int TotalSteps => Steps.Count;
    public int CompletedSteps => Steps.Count(s => s.Status == StepExecutionStatus.Completed);
    public int InProgressSteps => Steps.Count(s => s.Status == StepExecutionStatus.InProgress);
    public int FailedSteps => Steps.Count(s => s.Status == StepExecutionStatus.Failed);
    public int CancelledSteps => Steps.Count(s => s.Status == StepExecutionStatus.Cancelled);
    public int PendingSteps => Steps.Count(s => s.Status == StepExecutionStatus.Pending);
    
    public double ProgressPercentage => TotalSteps > 0 
        ? Math.Round((double)CompletedSteps / TotalSteps * 100, 2) 
        : 0;
}

/// <summary>
/// DTO for execution step information.
/// </summary>
public record ExecutionStepDto
{
    public Guid Id { get; init; }
    public int StepNumber { get; init; }
    public string ProcessName { get; init; } = string.Empty;
    public Guid ProposalId { get; init; }
    public Guid ProviderId { get; init; }
    public string ProviderName { get; init; } = string.Empty;
    public StepExecutionStatus Status { get; init; }
    
    public DateTime? ScheduledStart { get; init; }
    public DateTime? ScheduledEnd { get; init; }
    public TimeSpan? EstimatedDuration { get; init; }
    
    public decimal? EstimatedCost { get; init; }
}

/// <summary>
/// DTO for overall execution statistics.
/// </summary>
public record ExecutionSummaryDto
{
    public int TotalPlans { get; init; }
    public int InProgressPlans { get; init; }
    public int CompletedPlans { get; init; }
    public int FailedPlans { get; init; }
    public int ConfirmedPlans { get; init; }
    
    public List<ExecutionPlanSummaryDto> ActivePlans { get; init; } = [];
    public List<ExecutionPlanSummaryDto> RecentlyCompleted { get; init; } = [];
    public List<ExecutionPlanSummaryDto> Failed { get; init; } = [];
}
