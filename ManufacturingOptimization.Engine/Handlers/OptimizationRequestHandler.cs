using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Common.Models.Exceptions;
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
    private readonly IMapper _mapper;
    private readonly ILogger<OptimizationRequestHandler> _logger;

    public OptimizationRequestHandler(
        IWorkflowPipelineFactory pipelineFactory,
        ISystemReadinessService readinessService,
        IMessagePublisher messagePublisher,
        IMapper mapper,
        ILogger<OptimizationRequestHandler> logger)
    {
        _pipelineFactory = pipelineFactory;
        _readinessService = readinessService;
        _messagePublisher = messagePublisher;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task HandleAsync(RequestOptimizationPlanCommand command)
    {        
        // Wait for system to be ready before processing
        await _readinessService.WaitForSystemReadyAsync();
        await _readinessService.WaitForProvidersReadyAsync();

        var context = new WorkflowContext
        {
            Request = command.Request,
            Plan = command.Plan
        };

        try
        {
            var pipeline = _pipelineFactory.CreateOptimizationPipeline();
            await pipeline.ExecuteAsync(context);
        }
        catch (Exception ex)
        {
            context.Plan.ErrorMessage = ex.Message;
            context.Plan.Status = OptimizationPlanStatus.Failed;
            _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanUpdated, new OptimizationPlanUpdatedEvent
            {
                Plan = context.Plan
            });
        }
    }
}
