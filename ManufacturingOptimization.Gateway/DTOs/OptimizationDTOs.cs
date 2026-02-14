namespace ManufacturingOptimization.Gateway.DTOs;

public class OptimizationPlanPreviewDto
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

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
    public string? ErrorMessage { get; set; }
}

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

public class ProcessStepDto
{
    public Guid Id { get; set; }
    public int StepNumber { get; set; }
    public string Process { get; set; } = string.Empty;
    public Guid SelectedProviderId { get; set; }
    public string SelectedProviderName { get; set; } = string.Empty;
    public ProcessEstimateDto Estimate { get; set; } = new();
    public ProviderScheduleDto? AllocatedSchedule { get; set; }
    public ProviderScheduleDto? AllocatedSlot { get; set; }
}

public class ProcessEstimateDto
{
    public Guid Id { get; set; }
    public decimal Cost { get; set; }
    public double QualityScore { get; set; }
    public double EmissionsKgCO2 { get; set; }
    public double Duration { get; set; }
    public List<TimeWindowDto> AvailableTimeSlots { get; set; } = new();
}

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

public class WarrantyTermsDto
{
    public Guid Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public int DurationMonths { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IncludesInsurance { get; set; }
}
