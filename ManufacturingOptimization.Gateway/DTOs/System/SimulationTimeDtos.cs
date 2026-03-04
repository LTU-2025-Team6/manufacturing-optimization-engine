namespace ManufacturingOptimization.Gateway.DTOs.System;

/// <summary>
/// Current simulation time information.
/// </summary>
public class SimulationTimeDto
{
    public DateTime SimulatedUtcNow { get; set; }
    public DateTime RealUtcNow { get; set; }
    public double SpeedMultiplier { get; set; }
    public bool IsAccelerated { get; set; }
    public string FormattedSimulatedTime { get; set; } = string.Empty;
    public string FormattedRealTime { get; set; } = string.Empty;
}

/// <summary>
/// Request to set simulation time.
/// </summary>
public record SetSimulationTimeRequest(
    DateTime? SimulatedUtcNow,
    double? SpeedMultiplier
);
