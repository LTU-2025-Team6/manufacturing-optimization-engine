namespace ManufacturingOptimization.Common.Models.Enums;

/// <summary>
/// Status of the manufacturing plan execution.
/// </summary>
public enum OptimizationPlanStatus
{
    Draft,
    MatchingWorkflow,
    MatchingProviders,
    EstimatingCosts,
    GeneratingStrategies,
    AwaitingStrategySelection,
    StrategySelected,
    Ready,
    Confirmed,
    Failed,
    InProgress,
    Completed
}

