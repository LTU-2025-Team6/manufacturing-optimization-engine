using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.DTOs;
using ManufacturingOptimization.Gateway.Exceptions;

namespace ManufacturingOptimization.Gateway.Services
{
    public class OptimizationPlanService : IOptimizationPlanService
    {
        private readonly IMapper _mapper;
        private readonly IAsyncAwaiter _asyncAwaiter;
        private readonly IOptimizationPlanRepository _optimizationPlanRepository;
        private readonly IMessagePublisher _messagePublisher;

        public OptimizationPlanService(
            IMapper mapper,
            IAsyncAwaiter asyncAwaiter,
            IOptimizationPlanRepository optimizationPlanRepository,
            IMessagePublisher messagePublisher)
        {
            _mapper = mapper;
            _asyncAwaiter = asyncAwaiter;
            _optimizationPlanRepository = optimizationPlanRepository;
            _messagePublisher = messagePublisher;
        }

        public async Task<IEnumerable<OptimizationPlanPreviewDto>> GetAllAsync()
        {
            var plans = await _optimizationPlanRepository.GetAllAsync();

            plans.OrderByDescending(p => p.CreatedAt);

            return _mapper.Map<IEnumerable<OptimizationPlanPreviewDto>>(plans);
        }

        public async Task<OptimizationPlanDto> GetByIdAsync(Guid id)
        {
            var plan = await _optimizationPlanRepository.GetByIdAsync(id);

            if (plan == null)
                throw new NotFoundException($"Optimization plan with Id {id} not found.");

            return _mapper.Map<OptimizationPlanDto>(plan);
        }
    }
}
