using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Contracts;
using ManufacturingOptimization.Common.Enums;

namespace ManufacturingOptimization.Common.Messages;

public static class ProcessRoutingKeys
{
    public const string Propose = "simulatior.process.proposal";
    public const string Confirm = "simulatior.process.confirm";
    public const string Cancel = "simulatior.process.cancel";
    public const string Estimated = "simulatior.process.estimated";
    public const string Confirmed = "simulatior.process.reviewed";
    public const string Cancelled = "simulatior.process.cancelled";
    public const string ExecutionStarted = "process.execution.started";
    public const string ExecutionCompleted = "process.execution.completed";
}


public class ProposeProcessToProviderCommand : IMessage
{
    public Guid PlanId { get; set; }
    public Guid ProviderId { get; set; }
    public ProcessType Process { get; set; }
    public MotorSpecificationsModel MotorSpecs { get; set; } = null!;
    public TimeWindowModel RequestedTimeWindow { get; set; } = null!;
}


public class ProcessProposalEstimatedEvent : IMessage
{
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public bool Accepted { get; set; }
    public string? DeclineReason { get; set; }
    public Guid? ProposalId { get; set; }
    public ProcessEstimateModel? Estimate { get; set; }
    public ProviderScheduleModel? Schedule { get; set; }
}

public class ProcessProposalConfirmedEvent : IMessage
{
    public Guid ProposalId { get; set; }
    public bool IsAccepted { get; set; }
    public string? DeclineReason { get; set; }
}

public class ConfirmProcessProposalCommand : IMessage
{
    public Guid ProposalId { get; set; }
    public ProviderScheduleModel SelectedSchedule { get; set; } = null!;
}

public class CancelProcessCommand : IMessage
{
    public Guid ProposalId { get; set; }
}

public class ProcessCancelledEvent : IMessage
{
    public Guid ProposalId { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ProcessExecutionStartedEvent : IMessage
{
    public Guid PlanId { get; set; }
    public Guid ProposalId { get; set; }
    public Guid ProviderId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
}


public class ProcessExecutionCompletedEvent : IMessage
{
    public Guid PlanId { get; set; }
    public Guid ProposalId { get; set; }
    public Guid ProviderId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string FailureReason { get; set; } = string.Empty;
    public DateTime CompletedAt { get; set; }
}