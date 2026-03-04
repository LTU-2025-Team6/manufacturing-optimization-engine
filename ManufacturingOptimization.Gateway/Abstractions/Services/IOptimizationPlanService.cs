using ManufacturingOptimization.Gateway.DTOs.Common;
using ManufacturingOptimization.Gateway.DTOs.OptimizationPlan;

namespace ManufacturingOptimization.Gateway.Abstractions.Services
{
    public interface IOptimizationPlanService
    {
        Task<PagedResult<OptimizationPlanPreviewDto>> GetAllAsync(PaginationRequest pagination);
        Task<OptimizationPlanDto> GetByIdAsync(Guid id);
        Task SelectStrategyAsync(Guid planId, Guid strategyId);
        Task<CancelPlanResponse> CancelPlanAsync(Guid planId);
        Task DeletePlanAsync(Guid planId);
    }
}
