namespace ManufacturingOptimization.Gateway.DTOs.System;

/// <summary>
/// Current simulation time information.
/// </summary>
public class SimulationTimeDto
{
    public DateTime SimulatedUtcNow { get; set; }
    public double SpeedMultiplier { get; set; }
}

/// <summary>
/// Request to set simulation time.
/// DateTimeOffset preserves the client's timezone offset so .UtcDateTime always
/// produces the correct UTC value regardless of how the frontend serializes the time:
///   "08:50+02:00" → UtcDateTime = 06:50Z  ✓
///   "06:50Z"      → UtcDateTime = 06:50Z  ✓
///   "08:50"       → UtcDateTime = 08:50Z  (ambiguous, but consistent)
/// Using plain DateTime let the backend stamp bare "08:50" as 08:50Z UTC while
/// schedule segments were built from offset-aware inputs ("08:50+02:00" → 06:50Z),
/// producing a permanent 120-minute overdue gap.
/// </summary>
public record SetSimulationTimeRequest(
    DateTimeOffset? SimulatedUtcNow,
    double? SpeedMultiplier
);
