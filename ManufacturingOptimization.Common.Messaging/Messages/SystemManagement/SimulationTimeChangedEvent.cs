using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;

/// <summary>
/// Published when simulation time or speed is changed via API.
/// All services must synchronize their clocks to these values.
/// </summary>
public class SimulationTimeChangedEvent : BaseEvent
{
    /// <summary>
    /// The new simulation time that all services should set.
    /// </summary>
    public DateTime SimulatedUtcNow { get; set; }

    /// <summary>
    /// The new speed multiplier (1x = real-time, 100x = 100 times faster).
    /// </summary>
    public double SpeedMultiplier { get; set; }
}
