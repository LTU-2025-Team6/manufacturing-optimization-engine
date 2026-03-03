using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Gateway.Handlers;

public class ProcessExecutionCompletedEventHandler : IMessageHandler<ProcessExecutionCompletedEvent>
{
    private readonly IOptimizationPlanRepository _planRepository;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly ILogger<ProcessExecutionCompletedEventHandler> _logger;

    public ProcessExecutionCompletedEventHandler(
        IOptimizationPlanRepository planRepository,
        INotificationPublisher notificationPublisher,
        ILogger<ProcessExecutionCompletedEventHandler> logger)
    {
        _planRepository = planRepository;
        _notificationPublisher = notificationPublisher;
        _logger = logger;
    }

    public async Task HandleAsync(ProcessExecutionCompletedEvent @event)
    {
        // Get plan with strategies and steps
        var plan = await _planRepository.GetByIdAsync(@event.PlanId);
        if (plan == null)
            return;

        var strategy = plan.SelectedStrategy;
        if (strategy == null)
            return;

        // Find the step by ProposalId
        var step = strategy.Steps.FirstOrDefault(s => s.ProposalId == @event.ProposalId);
        if (step == null)
            return;

        // Idempotency check - ignore if step is already in a terminal state
        if (step.ExecutionStatus == StepExecutionStatus.Completed ||
            step.ExecutionStatus == StepExecutionStatus.Failed ||
            step.ExecutionStatus == StepExecutionStatus.Cancelled)
        {
            _logger.LogDebug("Step {StepId} already in terminal status {Status}, ignoring completion event",
                step.Id, step.ExecutionStatus);
            return;
        }

        if (@event.Success)
        {
            // Mark step as completed
            step.ExecutionStatus = StepExecutionStatus.Completed;

            // Check if all steps are completed
            var allStepsCompleted = strategy.Steps.All(s => s.ExecutionStatus == StepExecutionStatus.Completed);

            if (allStepsCompleted)
            {
                // Mark plan as completed
                plan.Status = OptimizationPlanStatus.Completed.ToString();
                plan.CompletedAt = @event.CompletedAt;

                _notificationPublisher.NotifyOptimizationCompleted(plan.Id);
            }
        }
        else
        {
            // Mark step as failed
            step.ExecutionStatus = StepExecutionStatus.Failed;

            // Only mark plan as failed if it's not already in a terminal state
            if (plan.Status != OptimizationPlanStatus.Failed.ToString() &&
                plan.Status != OptimizationPlanStatus.Completed.ToString())
            {
                plan.Status = OptimizationPlanStatus.Failed.ToString();
                plan.ErrorMessage = @event.FailureReason;
                plan.CompletedAt = @event.CompletedAt;

                // Send plan failure notification (only once)
                _notificationPublisher.NotifyOptimizationFailed(plan.Id, @event.FailureReason ?? "Unknown execution error");
            }

            // Cancel all other pending steps (just mark them as Cancelled, don't call CancelPlanAsync)
            foreach (var otherStep in strategy.Steps.Where(s => 
                s.Id != step.Id && s.ExecutionStatus == StepExecutionStatus.Pending))
            {
                otherStep.ExecutionStatus = StepExecutionStatus.Cancelled;
            }
        }

        await _planRepository.UpdateAsync(plan);
        await _planRepository.SaveChangesAsync();
    }
}
