using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.ExecutionManagement;

public class ExecutionStepStartedEvent : BaseEvent
{
    public Guid PlanId { get; set; }
    public Guid StepId { get; set; }
    public int StepNumber { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}