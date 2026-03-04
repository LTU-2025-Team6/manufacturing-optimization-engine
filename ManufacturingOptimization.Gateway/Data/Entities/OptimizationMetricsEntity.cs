namespace ManufacturingOptimization.Gateway.Data.Entities;

/// <summary>
/// Optimization metrics entity for database storage.
/// </summary>
public class OptimizationMetricsEntity
{
    public Guid Id { get; set; }
    public Guid StrategyId { get; set; }
    public decimal TotalCost { get; set; }
    public long TotalTime { get; set; } // Stored as Ticks
    public double AverageQuality { get; set; }
    public double TotalEmissionsKgCO2 { get; set; }
    public string SolverStatus { get; set; } = string.Empty;
    public double ObjectiveValue { get; set; }

    // Navigation property
    public OptimizationStrategyEntity Strategy { get; set; } = null!;
}
