namespace ManufacturingOptimization.Common.Models.DTOs;

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
