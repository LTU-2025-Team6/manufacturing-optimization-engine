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
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PlanExecutionCoordinator> _logger;

    public PlanExecutionCoordinator(
        IMessageSubscriber subscriber,
        IMessagingInfrastructure messagingInfrastructure,
        IServiceProvider serviceProvider,
        ILogger<PlanExecutionCoordinator> logger)
    {
        _subscriber = subscriber;
        _messagingInfrastructure = messagingInfrastructure;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queueName = "engine.execution.coordinator";

        // 1. Setup Infrastructure manually (Declare & Bind)
        _messagingInfrastructure.DeclareQueue(queueName);
        
        _messagingInfrastructure.BindQueue(
            queueName, 
            Exchanges.Optimization, 
            OptimizationRoutingKeys.PlanUpdated);

        // 2. Subscribe using the simple signature
        _subscriber.Subscribe<OptimizationPlanUpdatedEvent>(
            queueName,
            HandlePlanUpdatedAsync
        );

        return Task.CompletedTask;
    }

    // Changed to 'async void' to satisfy Action<T> delegate signature
    private async void HandlePlanUpdatedAsync(OptimizationPlanUpdatedEvent message)
    {
        // We only care if the plan was just CONFIRMED
        if (message.Plan.Status == OptimizationPlanStatus.Confirmed)
        {
            _logger.LogInformation("✅ Plan {PlanId} confirmed. Starting Execution Pipeline...", message.Plan.Id);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                
                var pipelineFactory = scope.ServiceProvider.GetRequiredService<IWorkflowPipelineFactory>();
                var executionPipeline = pipelineFactory.CreateExecutionPipeline();

                var context = new WorkflowContext
                {
                    Plan = message.Plan,
                    // Execution pipeline focuses on the Plan. 
                    // We set Request to null! to satisfy the required contract without fetching legacy data.
                    Request = null! 
                };

                await executionPipeline.ExecuteAsync(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to execute plan {PlanId}", message.Plan.Id);
            }
        }
    }
}