using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Enums;
using ManufacturingOptimization.Common.Exceptions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.OptimizationPipeline;

/// <summary>
/// Confirms accepted proposals with providers after strategy selection.
/// Sends final confirmation to providers that their proposals have been selected.
/// Works with ProcessStepEntity.
/// </summary>
public class FinalizationStep : IWorkflowStep
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly INotificationPublisher _notificationPublisher;

    public FinalizationStep(
        IMessagePublisher messagePublisher,
        INotificationPublisher notificationPublisher)
    {
        _messagePublisher = messagePublisher;
        _notificationPublisher = notificationPublisher;
    }

    public Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        if (context.Plan.SelectedStrategy == null)
            throw new OptimizationException("No strategy selected for confirmation. Please select a strategy before proceeding.");

        context.Plan.Status = OptimizationPlanStatus.Ready;

        _messagePublisher.Publish(
            Exchanges.Optimization,
            OptimizationRoutingKeys.PlanUpdated,
            new OptimizationPlanUpdatedEvent
            {
                Plan = context.Plan
            });

        return Task.CompletedTask;
    }
}
