namespace ManufacturingOptimization.Gateway.DTOs.OptimizationRequest;

/// <summary>
/// Optimization request DTO.
/// </summary>
public class OptimizationRequestDto
{
    public string CustomerId { get; set; } = string.Empty;
    public MotorSpecificationsDto MotorSpecs { get; set; } = null!;
    public OptimizationRequestConstraintsDto Constraints { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Motor specifications.
/// </summary>
public class MotorSpecificationsDto
{
    public double PowerKW { get; set; }
    public int AxisHeightMM { get; set; }
    public string CurrentEfficiency { get; set; } = string.Empty;
    public string TargetEfficiency { get; set; } = string.Empty;
    public string? MalfunctionDescription { get; set; }
}

/// <summary>
/// Optimization constraints.
/// </summary>
public class OptimizationRequestConstraintsDto
{
    public decimal? MaxBudget { get; set; }
    public TimeWindowDto TimeWindow { get; set; } = null!;
}

/// <summary>
/// Time window constraint.
/// </summary>
public class TimeWindowDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
