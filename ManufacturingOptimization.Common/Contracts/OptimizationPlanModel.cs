using ManufacturingOptimization.Common.Enums;

namespace ManufacturingOptimization.Common.Contracts;

public class OptimizationPlanModel
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public List<OptimizationStrategyModel> Strategies { get; set; } = [];
    public OptimizationStrategyModel? SelectedStrategy { get; set; }
    public OptimizationPlanStatus Status { get; set; } = OptimizationPlanStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SelectedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

