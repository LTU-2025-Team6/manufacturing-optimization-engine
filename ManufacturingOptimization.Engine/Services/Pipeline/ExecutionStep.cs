using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Common.Models.Exceptions;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

public class ExecutionStep : IWorkflowStep
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<ExecutionStep> _logger;

    public ExecutionStep(IMessagePublisher messagePublisher, ILogger<ExecutionStep> logger)
    {
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public string Name => "Process Execution";

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        if (context.Plan.SelectedStrategy == null)
            throw new OptimizationException("No strategy selected for execution.");

        // 1. Update Status to InProgress
        context.Plan.Status = OptimizationPlanStatus.InProgress;
        _logger.LogInformation("🚀 Starting execution for Plan {PlanId}", context.Plan.Id);

        // Notify the system (Frontend/Gateway) that execution has started
        _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanUpdated, new OptimizationPlanUpdatedEvent
        {
            Plan = context.Plan
        });

        // 2. Iterate through steps sequentially
        foreach (var step in context.Plan.SelectedStrategy.Steps.OrderBy(s => s.StepNumber))
        {
            _logger.LogInformation("👉 Executing Step {StepNumber}: {Process} with Provider {Provider}", 
                step.StepNumber, step.Process, step.SelectedProviderName);

            var command = new ExecuteProcessCommand
            {
                PlanId = context.Plan.Id,
                StepId = step.Id, 
                ProcessName = step.Process.ToString(),
                TargetProviderId = step.SelectedProviderId
            };

            // 3. Send Command and Wait for Reply
            // We expect a ProcessExecutionCompletedEvent back on a temporary queue
            var result = await _messagePublisher.RequestReplyAsync<ProcessExecutionCompletedEvent>(
                Exchanges.Process,
                $"process.execute.{step.SelectedProviderId}", // Routing key specific to the provider
                command,
                TimeSpan.FromSeconds(30)); // Timeout if provider doesn't respond

            if (result == null)
            {
                throw new OptimizationException($"Timeout waiting for Step {step.StepNumber} ({step.Process}) to complete.");
            }

            if (!result.Success)
            {
                throw new OptimizationException($"Execution failed at Step {step.StepNumber}: {result.FailureReason}");
            }

            _logger.LogInformation("✅ Step {StepNumber} Complete.", step.StepNumber);
        }

        // 4. Update Status to Completed
        context.Plan.Status = OptimizationPlanStatus.Completed;
        context.Plan.CompletedAt = DateTime.UtcNow;
        
        _logger.LogInformation("🏁 Plan {PlanId} COMPLETED!", context.Plan.Id);

        // Notify the system that the job is done
        _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanUpdated, new OptimizationPlanUpdatedEvent
        {
            Plan = context.Plan
        });
    }
}