using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;

/// <summary>
/// Propose a process to a provider for preliminary acceptance/rejection.
/// Provider can accept with estimates or decline.
/// </summary>
public class ProposeProcessToProviderCommand : BaseRequestReplyCommand
{
    public Guid PlanId { get; set; }
    public Guid ProviderId { get; set; }
    public ProcessType Process { get; set; }
    public MotorSpecificationsModel MotorSpecs { get; set; } = null!;
    public TimeWindowModel RequestedTimeWindow { get; set; } = null!;
}
