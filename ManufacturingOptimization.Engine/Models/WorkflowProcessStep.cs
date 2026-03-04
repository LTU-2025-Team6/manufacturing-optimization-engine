using ManufacturingOptimization.Common.Enums;

namespace ManufacturingOptimization.Engine.Models;

/// <summary>
/// Represents one manufacturing process step (e.g., Cleaning, Redesign, Turning).
/// </summary>
public class WorkflowProcessStep
{
    public int StepNumber { get; set; }
    public required ProcessType Process { get; init; }
    
    /// <summary>
    /// Providers that can perform this step.
    /// </summary>
    public List<MatchedProvider> MatchedProviders { get; set; } = new();
    
    /// <summary>
    /// Selected provider after optimization.
    /// </summary>
    public MatchedProvider? SelectedProvider { get; set; }
}
