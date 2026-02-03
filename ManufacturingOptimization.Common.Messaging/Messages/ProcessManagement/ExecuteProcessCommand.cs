using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
public class ExecuteProcessCommand : BaseRequestReplyCommand
{
    public Guid PlanId { get; set; }
    public Guid StepId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public Guid TargetProviderId { get; set; }
}