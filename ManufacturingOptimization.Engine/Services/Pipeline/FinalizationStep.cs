using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Common.Models.Exceptions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Confirms accepted proposals with providers after strategy selection.
/// Sends final confirmation to providers that their proposals have been selected.
/// Works with ProcessStepEntity.
/// </summary>
public class FinalizationStep : IWorkflowStep
{
    private readonly IMessagePublisher _messagePublisher;

    public FinalizationStep(IMessagePublisher messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }

    public string Name => "Process Finalization";

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        if (context.Plan.SelectedStrategy == null)
            throw new OptimizationException("No strategy selected for confirmation. Please select a strategy before proceeding.");

        context.Plan.Status = OptimizationPlanStatus.Ready;

        _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanUpdated, new OptimizationPlanUpdatedEvent
        {
            Plan = context.Plan
        });
    }
}
