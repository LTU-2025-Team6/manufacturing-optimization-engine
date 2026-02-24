using ManufacturingOptimization.Gateway.DTOs;

namespace ManufacturingOptimization.Gateway.Abstractions;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
}
