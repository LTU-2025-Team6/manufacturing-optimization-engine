using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.OptimizationPipeline;

/// <summary>
/// Simple pipeline executor - runs steps sequentially.
/// </summary>
public class WorkflowPipeline : IWorkflowPipeline
{
    private readonly IEnumerable<IWorkflowStep> _steps;
    private readonly ILogger<WorkflowPipeline> _logger;

    public WorkflowPipeline(
        IEnumerable<IWorkflowStep> steps,
        ILogger<WorkflowPipeline> logger)
    {
        _steps = steps;
        _logger = logger;
    }

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        foreach (var step in _steps)
        {
            await step.ExecuteAsync(context, cancellationToken);
        }
    }
}