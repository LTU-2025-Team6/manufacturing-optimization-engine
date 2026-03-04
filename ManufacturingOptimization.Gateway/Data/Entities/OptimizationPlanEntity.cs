namespace ManufacturingOptimization.Gateway.Data.Entities;

/// <summary>
/// Optimization plan entity for database storage.
/// </summary>
public class OptimizationPlanEntity
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public Guid? SelectedStrategyId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? SelectedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }

    // Navigation properties
    public List<OptimizationStrategyEntity> Strategies { get; set; } = [];
    public OptimizationStrategyEntity? SelectedStrategy { get; set; }
}