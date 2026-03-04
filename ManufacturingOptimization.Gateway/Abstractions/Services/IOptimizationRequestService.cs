using ManufacturingOptimization.Gateway.DTOs.OptimizationPlan;
using ManufacturingOptimization.Gateway.DTOs.OptimizationRequest;

namespace ManufacturingOptimization.Gateway.Abstractions.Services
{
    public interface IOptimizationRequestService
    {
        Task<Guid> RequestOptimizationPlanAsync(OptimizationRequestDto request);
        Task<OptimizationRequestDto> GetRequestAsync(Guid requestId);
    }
}
