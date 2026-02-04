using ManufacturingOptimization.Gateway.DTOs;

namespace ManufacturingOptimization.Gateway.Abstractions
{
    public interface IOptimizationService
    {
        Task<Guid> RequestOptimizationPlanAsync(OptimizationRequestDto request);
        Task SelectStrategyAsync(Guid requestId, Guid strategyId);
        Task<OptimizationPlanDto> GetPlanAsync(Guid requestId);
    }
}
