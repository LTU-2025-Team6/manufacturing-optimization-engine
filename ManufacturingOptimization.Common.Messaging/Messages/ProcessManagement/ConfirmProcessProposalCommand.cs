using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;

/// <summary>
/// Final confirmation of accepted process proposal to provider after strategy selection.
/// </summary>
public class ConfirmProcessProposalCommand : BaseRequestReplyCommand
{
    public Guid ProposalId { get; set; }
    public ProviderScheduleModel SelectedSchedule { get; set; } = null!;
}
