using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Abstractions;

/// <summary>
/// A single step in the workflow optimization pipeline.
/// </summary>
public interface IWorkflowStep
{
    Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default);
}
