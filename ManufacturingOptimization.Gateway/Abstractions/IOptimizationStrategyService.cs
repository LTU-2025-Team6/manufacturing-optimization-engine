using ManufacturingOptimization.Gateway.DTOs;

namespace ManufacturingOptimization.Gateway.Abstractions
{
    public interface IOptimizationStrategyService
    {
        Task<IEnumerable<AlternativeProviderDto>> GetAlternativeProviders(Guid strategyId, Guid stepId, DateTime requestedStartTime, DateTime requestedEndTime);
        Task<ValidateProcessTimeResponse> ValidateAlternativeProcessTime(Guid strategyId, Guid stepId, ValidateProcessTimeRequest request);
        Task<UpdateStrategyResponse> UpdateStrategy(Guid strategyId, UpdateStrategyRequest request);
        Task<ConfirmStrategyResponse> ConfirmStrategy(Guid strategyId);
    }
}
