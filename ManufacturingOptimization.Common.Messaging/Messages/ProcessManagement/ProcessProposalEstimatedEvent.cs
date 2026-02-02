using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models.Contracts;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;

/// <summary>
/// Response from provider regarding process proposal.
/// </summary>
public class ProcessProposalEstimatedEvent : BaseEvent
{
    public bool Accepted { get; set; }
    public string? DeclineReason { get; set; }
    public Guid? ProposalId { get; set; }
    public ProcessEstimateModel? Estimate { get; set; }
    public ProviderScheduleModel? Schedule { get; set; }
}
