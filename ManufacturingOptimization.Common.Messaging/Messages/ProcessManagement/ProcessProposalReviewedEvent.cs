using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;

/// <summary>
/// Confirmation from provider that the process has been accepted and scheduled.
/// </summary>
public class ProcessProposalReviewedEvent : BaseEvent
{
    public Guid ProposalId { get; set; }
    public bool IsAccepted { get; set; }
    public string? DeclineReason { get; set; }
}
