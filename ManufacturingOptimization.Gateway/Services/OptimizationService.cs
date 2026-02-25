using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.DTOs;
using ManufacturingOptimization.Gateway.Exceptions;

namespace ManufacturingOptimization.Gateway.Services
{
    public class OptimizationService : IOptimizationService
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly IOptimizationStrategyRepository _strategyRepository;
        private readonly IOptimizationPlanRepository _planRepository;
        private readonly IOptimizationRequestRepository _requestRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OptimizationService> _logger;

        public OptimizationService(
            IMessagePublisher messagePublisher,
            IOptimizationStrategyRepository strategyRepository,
            IOptimizationPlanRepository planRepository,
            IOptimizationRequestRepository requestRepository,
            IMapper mapper,
            ILogger<OptimizationService> logger)
        {
            _messagePublisher = messagePublisher;
            _strategyRepository = strategyRepository;
            _planRepository = planRepository;
            _requestRepository = requestRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Guid> RequestOptimizationPlanAsync(OptimizationRequestDto request)
        {
            // Save request to database
            var requestEntity = _mapper.Map<OptimizationRequestEntity>(request);
            await _requestRepository.AddAsync(requestEntity);
            await _requestRepository.SaveChangesAsync();

            // Map to model for messaging
            var requestModel = _mapper.Map<OptimizationRequestModel>(requestEntity);

            var planModel = new OptimizationPlanModel
            {
                RequestId = requestModel.RequestId,
                Status = Common.Models.Enums.OptimizationPlanStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };
            var planEntity = _mapper.Map<OptimizationPlanEntity>(planModel);

            await _planRepository.AddAsync(planEntity);
            await _planRepository.SaveChangesAsync();

            planModel = _mapper.Map<OptimizationPlanModel>(planEntity);

            var command = new RequestOptimizationPlanCommand
            {
                Request = requestModel,
                Plan = planModel
            };

            _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanRequested, command);

            return planModel.RequestId;
        }

        public Task SelectStrategyAsync(Guid requestId, Guid strategyId)
        {
            var command = new SelectStrategyCommand
            {
                RequestId = requestId,
                SelectedStrategyId = strategyId
            };
            var routingKey = $"{OptimizationRoutingKeys.StrategySelected}.{command.RequestId}";
            _messagePublisher.Publish(Exchanges.Optimization, routingKey, command);
            return Task.CompletedTask;
        }

        public async Task<OptimizationPlanDto> GetPlanAsync(Guid requestId)
        {
            var planEntity = await _planRepository.GetByRequestIdAsync(requestId);
            if (planEntity == null)
                throw new NotFoundException($"No optimization plan found for request {requestId}");

            var planModel = _mapper.Map<OptimizationPlanModel>(planEntity);
            var planDto = _mapper.Map<OptimizationPlanDto>(planModel);
            return planDto;
        }

        public async Task<OptimizationRequestDto> GetRequestAsync(Guid requestId)
        {
            var requestEntity = await _requestRepository.GetByIdWithDetailsAsync(requestId);
            if (requestEntity == null)
                throw new NotFoundException($"No optimization request found with ID {requestId}");

            var requestDto = _mapper.Map<OptimizationRequestDto>(requestEntity);
            return requestDto;
        }
    }
}
