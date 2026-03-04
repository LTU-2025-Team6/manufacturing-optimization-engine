using ManufacturingOptimization.Common.Contracts;

namespace ManufacturingOptimization.Engine.Models;

public class OptimizationResult
{
    /// <summary>
    /// Aggregated optimization metrics.
    /// </summary>
    public OptimizationMetricsModel Metrics { get; init; } = default!;

    /// <summary>
    /// Selected provider per workflow step.
    /// Key = StepNumber
    /// </summary>
    public Dictionary<int, MatchedProvider> SelectedProviders { get; init; } = [];
        
    /// <summary>
    /// Complete schedule with time allocations (when using time window optimization).
    /// </summary>
    public ScheduleTimeline? Timeline { get; init; }
}
