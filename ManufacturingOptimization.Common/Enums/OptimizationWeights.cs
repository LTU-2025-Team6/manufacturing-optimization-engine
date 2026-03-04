namespace ManufacturingOptimization.Common.Enums;

/// <summary>
/// Weights used in optimization objective function.
/// Each weight determines how much a specific metric influences the final decision.
/// </summary>
public class OptimizationWeights
{
    /// <summary>
    /// Weight for cost minimization (0.0 - 1.0)
    /// </summary>
    public double CostWeight { get; set; }

    /// <summary>
    /// Weight for time minimization (0.0 - 1.0)
    /// </summary>
    public double TimeWeight { get; set; }

    /// <summary>
    /// Weight for quality maximization (0.0 - 1.0)
    /// </summary>
    public double QualityWeight { get; set; }

    /// <summary>
    /// Weight for emissions minimization (0.0 - 1.0)
    /// </summary>
    public double EmissionsWeight { get; set; }
}
