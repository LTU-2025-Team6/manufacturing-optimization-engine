using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Services;

public class PlanExecutionCoordinator : BackgroundService
{
    private readonly IMessageSubscriber _subscriber;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PlanExecutionCoordinator> _logger;

    public PlanExecutionCoordinator(
        IMessageSubscriber subscriber,
        IServiceProvider serviceProvider,
        ILogger<PlanExecutionCoordinator> logger)
    {
        _subscriber = subscriber;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Subscribe to plan updates
        _subscriber.Subscribe<OptimizationPlanUpdatedEvent>(
            Exchanges.Optimization,
            OptimizationRoutingKeys.PlanUpdated,
            HandlePlanUpdatedAsync,
            "engine.execution.coordinator" // Unique queue for this service
        );

        return Task.CompletedTask;
    }

    private async Task HandlePlanUpdatedAsync(OptimizationPlanUpdatedEvent message)
    {
        // We only care if the plan was just CONFIRMED
        if (message.Plan.Status == OptimizationPlanStatus.Confirmed)
        {
            _logger.LogInformation("✅ Plan {PlanId} confirmed. Starting Execution Pipeline...", message.Plan.Id);

            try
            {
                // Create a new scope because we are in a BackgroundService
                using var scope = _serviceProvider.CreateScope();
                
                // 1. Get the Factory
                var pipelineFactory = scope.ServiceProvider.GetRequiredService<IWorkflowPipelineFactory>();
                
                // 2. Create the Execution Pipeline
                var executionPipeline = pipelineFactory.CreateExecutionPipeline();

                // 3. Create Context (Execution only needs the Plan)
                var context = new WorkflowContext
                {
                    Plan = message.Plan
                };

                // 4. Run the Pipeline
                await executionPipeline.ExecuteAsync(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to execute plan {PlanId}", message.Plan.Id);
            }
        }
    }
}