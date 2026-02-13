using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.ExecutionManagement;

public class ExecutionCompletedEvent : BaseEvent
{
    public Guid PlanId { get; set; }
    public Guid RequestId { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan TotalDuration { get; set; }
}