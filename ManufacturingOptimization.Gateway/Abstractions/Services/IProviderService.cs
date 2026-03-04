using ManufacturingOptimization.Gateway.DTOs.Common;
using ManufacturingOptimization.Gateway.DTOs.Provider;

namespace ManufacturingOptimization.Gateway.Abstractions.Services
{
    public interface IProviderService
    {
        Task<PagedResult<ProviderPreviewDto>> GetProvidersAsync(PaginationRequest pagination);
        Task<ProviderDto> GetProviderByIdAsync(Guid id);
        Task<ProviderDto> CreateProviderAsync(CreateProviderRequest request);
        Task<ProviderDto> UpdateProviderAsync(Guid id, UpdateProviderRequest request);
        Task DeleteProviderAsync(Guid id);
        Task<ProviderPreviewDto> ToggleProviderAsync(Guid id, bool isRunning);
        Task<List<ProviderDayScheduleDto>> GetProviderScheduleAsync(Guid providerId, ProviderScheduleRequest request);
        Task<ExecutionDetailsDto> GetExecutionDetailsAsync(Guid providerId, Guid executionId);
    }
}