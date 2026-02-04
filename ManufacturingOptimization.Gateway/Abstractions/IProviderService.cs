using ManufacturingOptimization.Gateway.DTOs;

namespace ManufacturingOptimization.Gateway.Abstractions
{
    public interface IProviderService
    {
        Task<List<ProviderPreviewDto>> GetProvidersAsync();
        Task<ProviderDto> GetProviderByIdAsync(Guid id);
        Task<ProviderDto> UpdateProviderAsync(Guid id, UpdateProviderRequest request);
        Task<ProviderPreviewDto> ToggleProviderAsync(Guid id, bool isRunning);
        Task<List<ProviderDayScheduleDto>> GetProviderScheduleAsync(Guid providerId, ProviderScheduleRequest request);
    }
}