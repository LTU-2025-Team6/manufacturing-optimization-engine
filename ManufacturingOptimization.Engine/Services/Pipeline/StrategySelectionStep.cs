using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagement;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Common.Models.Exceptions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Publishes generated strategies and waits for customer selection.
/// This step blocks until customer selects a strategy via SelectStrategyCommand.
/// Uses AutoMapper to convert Entity to Model for RabbitMQ messaging.
/// </summary>
public class StrategySelectionStep : IWorkflowStep
{
    private readonly TimeSpan TIMEOUT = TimeSpan.FromMinutes(10);
    private readonly IMessagePublisher _messagePublisher;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMapper _mapper;

    public string Name => "Strategy Selection";

    public StrategySelectionStep(
        IMessagePublisher messagePublisher,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMapper mapper)
    {
        _messagePublisher = messagePublisher;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _mapper = mapper;
    }

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        if (context.Plan.Strategies.Count == 0)
            throw new OptimizationException("No strategies available for selection");

        var requestId = context.Request.RequestId;

        // Create dedicated queue for this request's selection
        var selectionQueueName = $"engine.strategy.selection.{requestId}";
        var selectionRoutingKey = $"{OptimizationRoutingKeys.StrategySelected}.{requestId}";
        
        // TaskCompletionSource to wait for selection
        var selectionTcs = new TaskCompletionSource<SelectStrategyCommand>();

        // Setup temporary queue for this specific request
        _messagingInfrastructure.DeclareQueue(selectionQueueName);
        _messagingInfrastructure.BindQueue(selectionQueueName, Exchanges.Optimization, selectionRoutingKey);

        // Subscribe to selection for THIS request only
        _messageSubscriber.Subscribe<SelectStrategyCommand>(
            selectionQueueName,
            command =>
            {
                selectionTcs.TrySetResult(command);
            });

        // Update plan to AwaitingStrategySelection
        context.Plan.Status = OptimizationPlanStatus.AwaitingStrategySelection;
        _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanUpdated, new OptimizationPlanUpdatedEvent
        {
            Plan = context.Plan
        });

        // Wait for customer selection (with timeout)
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TIMEOUT);

        try
        {
            var selectionCommand = await selectionTcs.Task.WaitAsync(cts.Token);
            
            // Find the selected strategy in plan
            var selectedStrategy = context.Plan.Strategies.FirstOrDefault(s => s.Id == selectionCommand.SelectedStrategyId);

            if (selectedStrategy == null)
                throw new InvalidOperationException($"Selected strategy not found: {selectionCommand.SelectedStrategyId}");

            context.Plan.SelectedStrategy = selectedStrategy;
            context.Plan.SelectedAt = DateTime.UtcNow;
            context.Plan.Status = OptimizationPlanStatus.StrategySelected;
            _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanUpdated, new OptimizationPlanUpdatedEvent
            {
                Plan = context.Plan
            });
        }
        catch (OperationCanceledException) when (cts.Token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException("Customer selection timeout - no strategy selected within 10 minutes");
        }

        // Cleanup: purge temporary queue
        _messagingInfrastructure.PurgeQueue(selectionQueueName);
    }
}

