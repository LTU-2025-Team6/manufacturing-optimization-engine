using ManufacturingOptimization.Gateway.DTOs;

namespace ManufacturingOptimization.Gateway.Abstractions
{
    public interface IOptimizationPlanService
    {
        Task<IEnumerable<OptimizationPlanPreviewDto>> GetAllAsync();
        Task<OptimizationPlanDto> GetByIdAsync(Guid id);
        Task<CancelPlanResponse> CancelPlanAsync(Guid planId);
        Task DeletePlanAsync(Guid planId);
    }
}
