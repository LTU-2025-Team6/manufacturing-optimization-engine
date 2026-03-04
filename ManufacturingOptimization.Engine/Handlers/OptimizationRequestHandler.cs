using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Enums;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Handlers;

/// <summary>
/// Handles optimization plan requests by executing the workflow pipeline.
/// Registered as Scoped service - dependencies can be injected directly.
/// </summary>
public class OptimizationRequestHandler : IMessageHandler<RequestOptimizationPlanCommand>
{
    private readonly IWorkflowPipelineFactory _pipelineFactory;
    private readonly ISystemReadinessService _readinessService;
    private readonly IMessagePublisher _messagePublisher;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly ILogger<OptimizationRequestHandler> _logger;

    public OptimizationRequestHandler(
        IWorkflowPipelineFactory pipelineFactory,
        ISystemReadinessService readinessService,
        IMessagePublisher messagePublisher,
        INotificationPublisher notificationPublisher,
        ILogger<OptimizationRequestHandler> logger)
    {
        _pipelineFactory = pipelineFactory;
        _readinessService = readinessService;
        _messagePublisher = messagePublisher;
        _notificationPublisher = notificationPublisher;
        _logger = logger;
    }

    public async Task HandleAsync(RequestOptimizationPlanCommand command)
    {        
        // Wait for system to be ready before processing
        await _readinessService.WaitForSystemReadyAsync();
        await _readinessService.WaitForProvidersReadyAsync();

        // Notify that optimization has started
        _notificationPublisher.NotifyOptimizationStarted(command.Plan.Id);

        var context = new WorkflowContext
        {
            Request = command.Request,
            Plan = command.Plan
        };

        try
        {
            var pipeline = _pipelineFactory.CreateOptimizationPipeline();
            await pipeline.ExecuteAsync(context);

            _notificationPublisher.NotifyOptimizationCompleted(command.Plan.Id);
        }
        catch (Exception ex)
        {
            context.Plan.ErrorMessage = ex.Message;
            context.Plan.Status = OptimizationPlanStatus.Failed;

            _messagePublisher.Publish(
                Exchanges.Optimization,
                OptimizationRoutingKeys.PlanUpdated,
                new OptimizationPlanUpdatedEvent { Plan = context.Plan });

            _notificationPublisher.NotifyOptimizationFailed(command.Plan.Id, ex.Message);
        }
    }
}
