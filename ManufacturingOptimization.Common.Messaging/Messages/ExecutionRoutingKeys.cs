namespace ManufacturingOptimization.Common.Messaging.Messages;

public static class ExecutionRoutingKeys
{
    public const string ExecutionStarted = "execution.lifecycle.started";
    public const string ExecutionCompleted = "execution.lifecycle.completed";
    public const string ExecutionFailed = "execution.lifecycle.failed";
    
    public const string StepStarted = "execution.step.started";
    public const string StepCompleted = "execution.step.completed";
}