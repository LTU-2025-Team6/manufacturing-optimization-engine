using AutoMapper;
using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Common.Contracts;
using ManufacturingOptimization.ProviderSimulator.Abstractions;

namespace ManufacturingOptimization.ProviderSimulator.Handlers;

public sealed class RequestExecutionDetailsHandler : IMessageHandler<RequestExecutionDetailsCommand>
{
    private readonly IProviderSimulationContext _providerContext;
    private readonly IExecutionRepository _executionRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly ILogger<RequestExecutionDetailsHandler> _logger;
    private readonly IMapper _mapper;

    public RequestExecutionDetailsHandler(
        IProviderSimulationContext providerContext,
        IExecutionRepository executionRepository,
        IMessagePublisher messagePublisher,
        INotificationPublisher notificationPublisher,
        ILogger<RequestExecutionDetailsHandler> logger,
        IMapper mapper)
    {
        _providerContext = providerContext;
        _executionRepository = executionRepository;
        _messagePublisher = messagePublisher;
        _notificationPublisher = notificationPublisher;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task HandleAsync(RequestExecutionDetailsCommand command)
    {
        // Проверяем что запрос для этого провайдера
        if (command.ProviderId != _providerContext.Provider.Id)
            return;

        _logger.LogInformation("Provider {ProviderId} received execution details request for {ExecutionId}", 
            _providerContext.Provider.Id, command.ExecutionId);

        // Notify request received
        _notificationPublisher.NotifyProviderReceivedExecutionDetailsRequest(_providerContext.Provider.Name, command.ExecutionId);

        // Получаем execution с полными данными
        var execution = await _executionRepository.GetByIdWithDetailsAsync(command.ExecutionId);

        if (execution == null)
        {
            _logger.LogWarning("Execution {ExecutionId} not found", command.ExecutionId);
            return;
        }

        // Создаем детали execution
        var details = new ExecutionDetailsModel
        {
            ExecutionId = execution.Id,
            ProposalId = execution.ProposalId,
            PlanId = execution.Proposal.PlanId,
            ProviderId = execution.Proposal.ProviderId,
            Process = execution.Proposal.Process,
            Status = execution.Proposal.Status,
            ArrivedAt = execution.Proposal.ArrivedAt,
            ModifiedAt = execution.Proposal.ModifiedAt,
            MotorSpecs = _mapper.Map<MotorSpecificationsModel>(execution.Proposal.MotorSpecs),
            Estimate = execution.Proposal.Estimate != null 
                ? _mapper.Map<ProcessEstimateModel>(execution.Proposal.Estimate) 
                : null,
            ScheduleSegments = execution.ScheduleSegments
                .Select(s => new ExecutionTimeSlot
                {
                    Id = s.Id,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                })
                .ToList()
        };

        // Отправляем событие с деталями
        _messagePublisher.Publish(
            Exchanges.Provider, 
            ProviderRoutingKeys.ExecutionDetailsProvided, 
            new ExecutionDetailsProvidedEvent
            {
                ExecutionId = command.ExecutionId,
                Details = details
            });

        _logger.LogInformation("Provider {ProviderId} sent execution details for {ExecutionId}", 
            _providerContext.Provider.Id, command.ExecutionId);

        // Notify request completed
        _notificationPublisher.NotifyProviderCompletedExecutionDetailsRequest(_providerContext.Provider.Name, command.ExecutionId);
    }
}
