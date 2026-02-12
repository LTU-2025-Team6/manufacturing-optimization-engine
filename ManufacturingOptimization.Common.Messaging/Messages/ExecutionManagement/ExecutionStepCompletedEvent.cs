using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.ExecutionManagement;

public class ExecutionStepCompletedEvent : BaseEvent
{
    public Guid PlanId { get; set; }
    public Guid StepId { get; set; }
    public int StepNumber { get; set; }
    public bool Success { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}