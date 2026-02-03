using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;

public class ProcessExecutionCompletedEvent : BaseEvent
{
    public Guid PlanId { get; set; }
    public Guid StepId { get; set; }
    public Guid ProviderId { get; set; }
    public bool Success { get; set; }
    public string FailureReason { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}