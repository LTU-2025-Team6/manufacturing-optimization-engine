using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;

/// <summary>
/// Command to cancel a confirmed process proposal at a provider.
/// </summary>
public class CancelProcessCommand : BaseRequestReplyCommand
{
    public Guid ProposalId { get; set; }
}
