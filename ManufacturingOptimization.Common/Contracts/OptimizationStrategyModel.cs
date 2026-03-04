using ManufacturingOptimization.Common.Enums;

namespace ManufacturingOptimization.Common.Contracts;

public class OptimizationStrategyModel
{
    public Guid Id { get; set; }
    public Guid? PlanId { get; set; }
    public string StrategyName { get; set; } = string.Empty;
    public OptimizationPriority Priority { get; set; }
    public string WorkflowType { get; set; } = string.Empty;
    public List<ProcessStepModel> Steps { get; set; } = [];
    public OptimizationMetricsModel Metrics { get; set; } = new();
    public WarrantyTermsModel Warranty { get; set; } = new();
    public string Description { get; set; } = string.Empty;
}

