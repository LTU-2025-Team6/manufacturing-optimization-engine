using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.ExecutionManagement;

public class ExecutionStartedEvent : BaseEvent
{
    public Guid PlanId { get; set; }
    public Guid RequestId { get; set; }
    public int TotalSteps { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}