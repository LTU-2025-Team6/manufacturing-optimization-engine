using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Engine.Abstractions;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Factory for creating workflow processing pipelines with all required dependencies.
/// </summary>
public class PipelineFactory : IWorkflowPipelineFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IProviderRepository _providerRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMapper _mapper;

    public PipelineFactory(
        ILoggerFactory loggerFactory,
        IProviderRepository providerRepository,
        IMessagePublisher messagePublisher,
        IMessageSubscriber messageSubscriber,
        IMessagingInfrastructure messagingInfrastructure,
        IMapper mapper)
    {
        _loggerFactory = loggerFactory;
        _providerRepository = providerRepository;
        _messagePublisher = messagePublisher;
        _messageSubscriber = messageSubscriber;
        _messagingInfrastructure = messagingInfrastructure;
        _mapper = mapper;
    }

    public IWorkflowPipeline CreateWorkflowPipeline()
    {
        var steps = new IWorkflowStep[]
        {
            new WorkflowMatchingStep(_messagePublisher),
            new ProviderMatchingStep(_providerRepository, _messagePublisher),
            new EstimationStep(_messagePublisher),
            new OptimizationStep(_messagePublisher),
            new StrategySelectionStep(_messagePublisher, _messagingInfrastructure, _messageSubscriber, _mapper),
            new ConfirmationStep(_messagePublisher),
            
            new ExecutionStep(_messagePublisher, _loggerFactory.CreateLogger<ExecutionStep>())
        };

        return new WorkflowPipeline(steps, _loggerFactory.CreateLogger<WorkflowPipeline>());
    }
}