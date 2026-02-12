namespace ManufacturingOptimization.Engine.Abstractions;

/// <summary>
/// Factory for creating workflow processing pipelines.
/// </summary>
public interface IWorkflowPipelineFactory
{
    /// <summary>
    /// Creates two new workflow pipelines instances.
    /// </summary>
    IWorkflowPipeline CreateOptimizationPipeline(); 
    IWorkflowPipeline CreateExecutionPipeline();
}
