using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ExecutionManagement;
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

        var startTime = DateTime.UtcNow;

        _messagePublisher.Publish(Exchanges.Execution, ExecutionRoutingKeys.ExecutionStarted, new ExecutionStartedEvent
        {
            PlanId = context.Plan.Id,
            RequestId = context.Plan.RequestId,
            TotalSteps = context.Plan.SelectedStrategy.Steps.Count
        });

        // Update Status to InProgress
        context.Plan.Status = OptimizationPlanStatus.InProgress;
        _logger.LogInformation("🚀 Starting execution for Plan {PlanId}", context.Plan.Id);

        // Notify system (Legacy/UI update)
        _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanUpdated, new OptimizationPlanUpdatedEvent
        {
            Plan = context.Plan
        });

        try
        {
            // Iterate through steps sequentially
            foreach (var step in context.Plan.SelectedStrategy.Steps.OrderBy(s => s.StepNumber))
            {
                _logger.LogInformation("👉 Executing Step {StepNumber}: {Process} with Provider {Provider}", 
                    step.StepNumber, step.Process, step.SelectedProviderName);

                _messagePublisher.Publish(Exchanges.Execution, ExecutionRoutingKeys.StepStarted, new ExecutionStepStartedEvent
                {
                    PlanId = context.Plan.Id,
                    StepId = step.Id,
                    StepNumber = step.StepNumber,
                    ProcessName = step.Process.ToString(),
                    ProviderId = step.SelectedProviderId,
                    ProviderName = step.SelectedProviderName
                });

                var command = new ExecuteProcessCommand
                {
                    PlanId = context.Plan.Id,
                    StepId = step.Id, 
                    ProcessName = step.Process.ToString(),
                    TargetProviderId = step.SelectedProviderId
                };

                // Send Command and Wait for Reply (RPC Style)
                var result = await _messagePublisher.RequestReplyAsync<ProcessExecutionCompletedEvent>(
                    Exchanges.Process,
                    $"process.execute.{step.SelectedProviderId}", 
                    command,
                    TimeSpan.FromSeconds(30)); 

                if (result == null)
                {
                    throw new OptimizationException($"Timeout waiting for Step {step.StepNumber} ({step.Process}) to complete.");
                }

                if (!result.Success)
                {
                    throw new OptimizationException($"Execution failed at Step {step.StepNumber}: {result.FailureReason}");
                }

                _messagePublisher.Publish(Exchanges.Execution, ExecutionRoutingKeys.StepCompleted, new ExecutionStepCompletedEvent
                {
                    PlanId = context.Plan.Id,
                    StepId = step.Id,
                    StepNumber = step.StepNumber,
                    Success = true
                });

                _logger.LogInformation("✅ Step {StepNumber} Complete.", step.StepNumber);
            }

            // Update Status to Completed
            context.Plan.Status = OptimizationPlanStatus.Completed;
            context.Plan.CompletedAt = DateTime.UtcNow;
            
            _messagePublisher.Publish(Exchanges.Execution, ExecutionRoutingKeys.ExecutionCompleted, new ExecutionCompletedEvent
            {
                PlanId = context.Plan.Id,
                RequestId = context.Plan.RequestId,
                TotalDuration = DateTime.UtcNow - startTime
            });

            _logger.LogInformation("🏁 Plan {PlanId} COMPLETED!", context.Plan.Id);

            _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanUpdated, new OptimizationPlanUpdatedEvent
            {
                Plan = context.Plan
            });
        }
        catch (Exception ex)
        {
            _messagePublisher.Publish(Exchanges.Execution, ExecutionRoutingKeys.ExecutionFailed, new ExecutionFailedEvent
            {
                PlanId = context.Plan.Id,
                Reason = ex.Message
            });
            throw; // Re-throw so the pipeline handler knows it failed
        }
    }
}