using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;

/// <summary>
/// Published when a provider actually starts executing a confirmed process.
/// </summary>
public class ProcessExecutionStartedEvent : BaseEvent
{
    public Guid PlanId { get; set; }
    public Guid ProposalId { get; set; }
    public Guid ProviderId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
}
