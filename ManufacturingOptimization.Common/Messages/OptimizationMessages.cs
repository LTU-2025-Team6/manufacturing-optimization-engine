using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Contracts;

namespace ManufacturingOptimization.Common.Messages;

public static class OptimizationRoutingKeys
{
    public const string PlanRequested = "optimization.plan-requested";
    public const string PlanUpdated = "optimization.plan-updated";
    public const string StrategySelected = "optimization.strategy.selected";
}


public class RequestOptimizationPlanCommand : IMessage
{
    public OptimizationRequestModel Request { get; set; } = null!;
    public OptimizationPlanModel Plan { get; set; } = null!;
}

public class SelectStrategyCommand : IMessage
{
    public Guid RequestId { get; set; }
    public Guid SelectedStrategyId { get; set; }
    public string SelectedStrategyName { get; set; } = string.Empty;
    public DateTime SelectedAt { get; set; } = DateTime.UtcNow;
}

public class OptimizationPlanUpdatedEvent : IMessage
{
    public OptimizationPlanModel Plan { get; set; } = null!;
}
