namespace ManufacturingOptimization.Common.Models.Data.Entities;

/// <summary>
/// Optimization request entity for database storage.
/// </summary>
public class OptimizationRequestEntity
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Owned entities
    public MotorSpecificationsEntity MotorSpecs { get; set; } = null!;
    public OptimizationRequestConstraintsEntity Constraints { get; set; } = null!;
}

/// <summary>
/// Motor specifications owned entity.
/// </summary>
public class MotorSpecificationsEntity
{
    public double PowerKW { get; set; }
    public int AxisHeightMM { get; set; }
    public string CurrentEfficiency { get; set; } = string.Empty;
    public string TargetEfficiency { get; set; } = string.Empty;
    public string? MalfunctionDescription { get; set; }
}

/// <summary>
/// Optimization request constraints owned entity.
/// </summary>
public class OptimizationRequestConstraintsEntity
{
    public decimal? MaxBudget { get; set; }
    public TimeWindowEntity TimeWindow { get; set; } = null!;
}

/// <summary>
/// Time window owned entity.
/// </summary>
public class TimeWindowEntity
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

