using ManufacturingOptimization.Gateway.DTOs.Provider;

namespace ManufacturingOptimization.Gateway.DTOs.OptimizationPlan;

/// <summary>
/// Preview information about an optimization plan for list views.
/// </summary>
public class OptimizationPlanPreviewDto
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Complete optimization plan with all strategies.
/// </summary>
public class OptimizationPlanDto
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public List<OptimizationStrategyDto> Strategies { get; set; } = [];
    public OptimizationStrategyDto? SelectedStrategy { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? SelectedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Optimization strategy with process steps and metrics.
/// </summary>
public class OptimizationStrategyDto
{
    public Guid Id { get; set; }
    public Guid? PlanId { get; set; }
    public string StrategyName { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string WorkflowType { get; set; } = string.Empty;
    public List<ProcessStepDto> Steps { get; set; } = new();
    public OptimizationMetricsDto Metrics { get; set; } = new();
    public WarrantyTermsDto Warranty { get; set; } = new();
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Process step within a strategy.
/// </summary>
public class ProcessStepDto
{
    public Guid Id { get; set; }
    public int StepNumber { get; set; }
    public string Process { get; set; } = string.Empty;
    public Guid SelectedProviderId { get; set; }
    public string SelectedProviderName { get; set; } = string.Empty;
    public string ExecutionStatus { get; set; } = "Pending";
    public ProcessEstimateDto Estimate { get; set; } = new();
    public ProviderScheduleDto? AllocatedSchedule { get; set; }
}

/// <summary>
/// Optimization metrics for a strategy.
/// </summary>
public class OptimizationMetricsDto
{
    public Guid Id { get; set; }
    public decimal TotalCost { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public double AverageQuality { get; set; }
    public double TotalEmissionsKgCO2 { get; set; }
    public string SolverStatus { get; set; } = string.Empty;
    public double ObjectiveValue { get; set; }
}

/// <summary>
/// Warranty terms for a strategy.
/// </summary>
public class WarrantyTermsDto
{
    public Guid Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public int DurationMonths { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IncludesInsurance { get; set; }
}
