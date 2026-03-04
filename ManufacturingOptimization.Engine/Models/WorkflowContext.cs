using ManufacturingOptimization.Common.Contracts;

namespace ManufacturingOptimization.Engine.Models;

/// <summary>
/// Context flowing through the workflow optimization pipeline.
/// Works with Entity classes internally.
/// </summary>
public class WorkflowContext
{
    public required OptimizationRequestModel Request { get; init; }
    
    /// <summary>
    /// Optimization plan being built during pipeline execution.
    /// Created at the start with Draft status and progressively filled.
    /// </summary>
    public required OptimizationPlanModel Plan { get; init; }
    
    /// <summary>
    /// Type of workflow: "Upgrade" or "Refurbish"
    /// </summary>
    public string? WorkflowType { get; set; }
    
    /// <summary>
    /// Sequential manufacturing process steps.
    /// Upgrade: 8 steps, Refurbish: 5 steps
    /// </summary>
    public List<WorkflowProcessStep> ProcessSteps { get; set; } = [];
}

