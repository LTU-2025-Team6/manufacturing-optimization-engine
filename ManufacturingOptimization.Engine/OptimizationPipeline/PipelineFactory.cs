using ManufacturingOptimization.Engine.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ManufacturingOptimization.Engine.OptimizationPipeline;

/// <summary>
/// Factory for creating workflow processing pipelines.
/// Uses DI container to resolve all step dependencies automatically.
/// </summary>
public class PipelineFactory : IWorkflowPipelineFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerFactory _loggerFactory;

    public PipelineFactory(
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory)
    {
        _serviceProvider = serviceProvider;
        _loggerFactory = loggerFactory;
    }

    public IWorkflowPipeline CreateOptimizationPipeline()
    {
        var steps = new IWorkflowStep[]
        {
            _serviceProvider.GetRequiredService<WorkflowMatchingStep>(),
            _serviceProvider.GetRequiredService<ProviderMatchingStep>(),
            _serviceProvider.GetRequiredService<EstimationStep>(),
            _serviceProvider.GetRequiredService<OptimizationStep>(),
            _serviceProvider.GetRequiredService<StrategySelectionStep>(),
            _serviceProvider.GetRequiredService<FinalizationStep>()
        };

        return new WorkflowPipeline(steps, _loggerFactory.CreateLogger<WorkflowPipeline>());
    }
}