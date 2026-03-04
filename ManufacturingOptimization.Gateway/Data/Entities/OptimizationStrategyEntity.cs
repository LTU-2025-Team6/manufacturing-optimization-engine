namespace ManufacturingOptimization.Gateway.Data.Entities;

/// <summary>
/// Optimization strategy entity for database storage.
/// </summary>
public class OptimizationStrategyEntity
{
    public Guid Id { get; set; }
    public Guid? PlanId { get; set; }
    public string StrategyName { get; set; } = string.Empty;
    public string WorkflowType { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }

    // Navigation properties
    public OptimizationPlanEntity? Plan { get; set; }
    public ICollection<ProcessStepEntity> Steps { get; set; } = [];
    public OptimizationMetricsEntity? Metrics { get; set; }
    public WarrantyTermsEntity? Warranty { get; set; }
}
