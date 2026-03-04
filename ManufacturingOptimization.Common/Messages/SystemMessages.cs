using ManufacturingOptimization.Common.Abstractions;

namespace ManufacturingOptimization.Common.Messages;

public static class SystemRoutingKeys
{
    public const string ServiceReady = "system.service.ready";
    public const string SystemReady = "system.ready";
    public const string TimeChanged = "system.time.changed";
}


public class ServiceReadyEvent : IMessage
{
    public string ServiceName { get; set; } = string.Empty;
}

public class SystemReadyEvent : IMessage
{
    public List<string> ReadyServices { get; set; } = new();
}

public class SimulationTimeChangedEvent : IMessage
{
    public DateTime SimulatedUtcNow { get; set; }
    public double SpeedMultiplier { get; set; }
}
