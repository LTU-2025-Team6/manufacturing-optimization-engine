using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Enums;
using ManufacturingOptimization.Common.Exceptions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.OptimizationPipeline;

/// <summary>
/// Publishes generated strategies and waits for customer selection.
/// This step blocks until customer selects a strategy via SelectStrategyCommand.
/// </summary>
public class StrategySelectionStep : IWorkflowStep
{
    private readonly TimeSpan TIMEOUT = TimeSpan.FromMinutes(10);
    private readonly IAsyncAwaiter _asyncAwaiter;
    private readonly IMessagePublisher _messagePublisher;
    private readonly INotificationPublisher _notificationPublisher;

    public StrategySelectionStep(
        IAsyncAwaiter asyncAwaiter,
        IMessagePublisher messagePublisher,
        INotificationPublisher notificationPublisher)
    {
        _asyncAwaiter = asyncAwaiter;
        _messagePublisher = messagePublisher;
        _notificationPublisher = notificationPublisher;
    }

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        _notificationPublisher.NotifyOptimizationStepStarted("Strategy Selection", context.Plan.Id);

        if (context.Plan.Strategies.Count == 0)
            throw new OptimizationException("No strategies available for selection");

        var requestId = context.Request.RequestId;

        SelectStrategyCommand selectionCommand;
        try
        {
            selectionCommand = await _asyncAwaiter.AwaitAsync(new AwaitScenario<SelectStrategyCommand>
            {
                Exchange = Exchanges.Optimization,
                RoutingKey = OptimizationRoutingKeys.StrategySelected,
                Timeout = TIMEOUT,
                Match = cmd => cmd.RequestId == requestId,
                BeforeAwait = () =>
                {
                    context.Plan.Status = OptimizationPlanStatus.AwaitingStrategySelection;

                    _messagePublisher.Publish(
                        Exchanges.Optimization,
                        OptimizationRoutingKeys.PlanUpdated,
                        new OptimizationPlanUpdatedEvent { Plan = context.Plan });
                }
            });
        }
        catch (TimeoutException)
        {
            throw new InvalidOperationException("Customer selection timeout - no strategy selected within 10 minutes");
        }

        var selectedStrategy = context.Plan.Strategies.FirstOrDefault(s => s.Id == selectionCommand.SelectedStrategyId)
            ?? throw new InvalidOperationException($"Selected strategy not found: {selectionCommand.SelectedStrategyId}");

        context.Plan.SelectedStrategy = selectedStrategy;
        context.Plan.SelectedAt = DateTime.UtcNow;
        context.Plan.Status = OptimizationPlanStatus.StrategySelected;

        _messagePublisher.Publish(
            Exchanges.Optimization,
            OptimizationRoutingKeys.PlanUpdated,
            new OptimizationPlanUpdatedEvent { Plan = context.Plan });

        _notificationPublisher.NotifyStrategySelected(context.Plan.Id, context.Plan.SelectedStrategy.Id);
    }
}
