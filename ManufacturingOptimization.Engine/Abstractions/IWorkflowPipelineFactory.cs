namespace ManufacturingOptimization.Engine.Abstractions;

/// <summary>
/// Factory for creating workflow processing pipelines.
/// </summary>
public interface IWorkflowPipelineFactory
{
    IWorkflowPipeline CreateOptimizationPipeline(); 
}
