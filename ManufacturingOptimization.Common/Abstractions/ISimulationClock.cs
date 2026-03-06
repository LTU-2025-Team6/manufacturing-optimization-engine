namespace ManufacturingOptimization.Common.Abstractions;

/// <summary>
/// Global simulation clock for all services.
/// Provides current simulated time and speed control.
/// Defaults to real UTC time at ×1 speed on startup.
/// Updated by Gateway via RabbitMQ SimulationTimeChangedEvent.
/// </summary>
public interface ISimulationClock
{
    /// <summary>Current simulated UTC time.</summary>
    DateTime UtcNow { get; }

    /// <summary>Current speed multiplier (1× = real-time, 10× = 10 times faster, etc.)</summary>
    double SpeedMultiplier { get; }

    /// <summary>Set the current simulated time and speed multiplier.</summary>
    void SetTime(DateTime simulatedUtcNow, double speedMultiplier);
}
