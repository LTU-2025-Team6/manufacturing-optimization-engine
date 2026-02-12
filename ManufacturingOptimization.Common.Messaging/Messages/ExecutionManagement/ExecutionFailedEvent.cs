using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.ExecutionManagement;

public class ExecutionFailedEvent : BaseEvent
{
    public Guid PlanId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; } = DateTime.UtcNow;
}