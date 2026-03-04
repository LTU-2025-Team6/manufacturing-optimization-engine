using ManufacturingOptimization.Gateway.DTOs.Provider;
using ManufacturingOptimization.Gateway.DTOs.Strategy;

namespace ManufacturingOptimization.Gateway.Abstractions.Services;

public interface IOptimizationStrategyService
{
    Task<IEnumerable<AlternativeProviderDto>> GetAlternativeProvidersAsync(Guid planId, Guid stepId, DateTime windowStart, DateTime windowEnd);
    Task<ValidateSlotResponse> ValidateSlotAsync(Guid planId, Guid stepId, ValidateSlotRequest request);
    Task<UpdateStrategyResponse> UpdateStrategyAsync(Guid planId, UpdateStrategyRequest request);
    Task<ConfirmStrategyResponse> ConfirmStrategy(Guid strategyId);
}
