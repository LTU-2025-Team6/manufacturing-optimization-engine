using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Gateway.Handlers;

/// <summary>
/// Handles process execution events from providers.
/// Updates plan status, sends notifications, and manages plan lifecycle.
/// </summary>
public class ProcessExecutionStartedEventHandler : IMessageHandler<ProcessExecutionStartedEvent>
{
    private readonly IOptimizationPlanRepository _planRepository;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly ILogger<ProcessExecutionStartedEventHandler> _logger;

    public ProcessExecutionStartedEventHandler(
        IOptimizationPlanRepository planRepository,
        IServiceScopeFactory scopeFactory,
        INotificationPublisher notificationPublisher,
        ILogger<ProcessExecutionStartedEventHandler> logger)
    {
        _planRepository = planRepository;
        _notificationPublisher = notificationPublisher;
        _logger = logger;
    }

    public async Task HandleAsync(ProcessExecutionStartedEvent @event)
    {
        // Get plan with strategies and steps
        var plan = await _planRepository.GetByIdAsync(@event.PlanId);
        if (plan == null)
            return;

        // Find the step by ProposalId
        var strategy = plan.SelectedStrategy;
        if (strategy == null)
            return;

        var step = strategy.Steps.FirstOrDefault(s => s.ProposalId == @event.ProposalId);
        if (step == null)
            return;

        // Idempotency check - ignore if step is already InProgress or in a terminal state
        if (step.ExecutionStatus == StepExecutionStatus.InProgress ||
            step.ExecutionStatus == StepExecutionStatus.Completed ||
            step.ExecutionStatus == StepExecutionStatus.Failed ||
            step.ExecutionStatus == StepExecutionStatus.Cancelled)
        {
            _logger.LogDebug("Step {StepId} already in status {Status}, ignoring start event",
                step.Id, step.ExecutionStatus);
            return;
        }

        // Update step status
        step.ExecutionStatus = StepExecutionStatus.InProgress;

        await _planRepository.UpdateAsync(plan);
        await _planRepository.SaveChangesAsync();
    }
}
