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
    private readonly IAsyncAwaiter _asyncAwaiter;
    private readonly IMessagePublisher _messagePublisher;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMapper _mapper;

    public PipelineFactory(
        ILoggerFactory loggerFactory,
        IAsyncAwaiter asyncAwaiter,
        IProviderRepository providerRepository,
        IMessagePublisher messagePublisher,
        INotificationPublisher notificationPublisher,
        IMessageSubscriber messageSubscriber,
        IMessagingInfrastructure messagingInfrastructure,
        IMapper mapper)
    {
        _loggerFactory = loggerFactory;
        _providerRepository = providerRepository;
        _asyncAwaiter = asyncAwaiter;
        _messagePublisher = messagePublisher;
        _notificationPublisher = notificationPublisher;
        _messageSubscriber = messageSubscriber;
        _messagingInfrastructure = messagingInfrastructure;
        _mapper = mapper;
    }

    public IWorkflowPipeline CreateWorkflowPipeline()
    {
        var steps = new IWorkflowStep[]
        {
            new WorkflowMatchingStep(_messagePublisher, _notificationPublisher),
            new ProviderMatchingStep(_providerRepository, _messagePublisher, _notificationPublisher),
            new EstimationStep(_messagePublisher, _notificationPublisher, _asyncAwaiter),
            new OptimizationStep(_messagePublisher, _notificationPublisher),
            new StrategySelectionStep(_messagePublisher, _notificationPublisher, _messagingInfrastructure, _messageSubscriber, _mapper),
            new FinalizationStep(_messagePublisher, _notificationPublisher)
        };

        return new WorkflowPipeline(steps, _loggerFactory.CreateLogger<WorkflowPipeline>());
    }
}

