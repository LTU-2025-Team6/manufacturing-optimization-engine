using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;

/// <summary>
/// Event published when a process has been successfully cancelled at a provider.
/// </summary>
public class ProcessCancelledEvent : IMessage
{
    public Guid ProposalId { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
