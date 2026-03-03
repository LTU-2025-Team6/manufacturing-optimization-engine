namespace ManufacturingOptimization.Common.Messaging.Messages;

public static class ProcessRoutingKeys
{
    public const string Propose = "simulatior.process.proposal";
    public const string Confirm = "simulatior.process.confirm";
    public const string Cancel = "simulatior.process.cancel";
    public const string Estimated = "simulatior.process.estimated";
    public const string Reviewed = "simulatior.process.reviewed";
    public const string Cancelled = "simulatior.process.cancelled";
    public const string ExecutionStarted = "process.execution.started";
    public const string ExecutionCompleted = "process.execution.completed";
}
