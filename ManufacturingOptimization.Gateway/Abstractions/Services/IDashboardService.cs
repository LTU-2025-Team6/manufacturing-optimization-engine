using ManufacturingOptimization.Gateway.DTOs.Dashboard;

namespace ManufacturingOptimization.Gateway.Abstractions.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
}
