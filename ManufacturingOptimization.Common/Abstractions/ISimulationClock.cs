namespace ManufacturingOptimization.Common.Abstractions;

/// <summary>
/// Global simulation clock for all services.
/// Provides current simulated time and speed control.
/// </summary>
public interface ISimulationClock
{
    /// <summary>
    /// Current simulated UTC time.
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Current speed multiplier (1x = real-time, 10x = 10 times faster, etc.)
    /// </summary>
    double SpeedMultiplier { get; }

    /// <summary>
    /// Set the current simulated time and speed multiplier.
    /// </summary>
    void SetTime(DateTime simulatedUtcNow, double speedMultiplier);
}
