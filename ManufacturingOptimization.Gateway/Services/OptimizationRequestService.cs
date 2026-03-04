using AutoMapper;
using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Contracts;
using ManufacturingOptimization.Common.Enums;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;
using ManufacturingOptimization.Gateway.Abstractions.Services;
using ManufacturingOptimization.Gateway.Data.Entities;
using ManufacturingOptimization.Gateway.DTOs.OptimizationRequest;
using ManufacturingOptimization.Gateway.Exceptions;

namespace ManufacturingOptimization.Gateway.Services;

public class OptimizationRequestService : IOptimizationRequestService
{
        private readonly IMessagePublisher _messagePublisher;
        private readonly IOptimizationStrategyRepository _strategyRepository;
        private readonly IOptimizationPlanRepository _planRepository;
        private readonly IOptimizationRequestRepository _requestRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OptimizationRequestService> _logger;

        public OptimizationRequestService(
            IMessagePublisher messagePublisher,
            IOptimizationStrategyRepository strategyRepository,
            IOptimizationPlanRepository planRepository,
            IOptimizationRequestRepository requestRepository,
            IMapper mapper,
            ILogger<OptimizationRequestService> logger)
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
                Status = OptimizationPlanStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };
            var planEntity = _mapper.Map<OptimizationPlanEntity>(planModel);

            await _planRepository.AddAsync(planEntity);
            await _planRepository.SaveChangesAsync();

            planModel = _mapper.Map<OptimizationPlanModel>(planEntity);

            _messagePublisher.Publish(
                Exchanges.Optimization,
                OptimizationRoutingKeys.PlanRequested,
                new RequestOptimizationPlanCommand
                {
                    Request = requestModel,
                    Plan = planModel
                });

            return planModel.Id;
        }

        public async Task<OptimizationRequestDto> GetRequestAsync(Guid requestId)
        {
            var requestEntity = await _requestRepository.GetByIdWithDetailsAsync(requestId)
                ?? throw new NotFoundException($"Optimization request with Id {requestId} not found.");

            return _mapper.Map<OptimizationRequestDto>(requestEntity);
        }
}
