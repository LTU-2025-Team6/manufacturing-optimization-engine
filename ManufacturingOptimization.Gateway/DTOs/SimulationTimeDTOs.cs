namespace ManufacturingOptimization.Gateway.DTOs;

/// <summary>
/// Current simulation time state.
/// </summary>
public class SimulationTimeDto
{
    /// <summary>
    /// Current simulated UTC time.
    /// </summary>
    public DateTime SimulatedUtcNow { get; set; }

    /// <summary>
    /// Current speed multiplier (1x = real-time, 100x = 100 times faster).
    /// </summary>
    public double SpeedMultiplier { get; set; }
}

/// <summary>
/// Request to update simulation time and/or speed.
/// </summary>
public record SetSimulationTimeRequest(
    DateTime? SimulatedUtcNow,
    double? SpeedMultiplier
);
