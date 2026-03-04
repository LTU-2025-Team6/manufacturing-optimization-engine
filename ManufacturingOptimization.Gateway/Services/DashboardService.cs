using ManufacturingOptimization.Gateway.DTOs.Dashboard;
using ManufacturingOptimization.Gateway.Abstractions.Services;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;

namespace ManufacturingOptimization.Gateway.Services;

public class DashboardService : IDashboardService
{
    private readonly IOptimizationPlanRepository _optimizationPlanRepository;
    private readonly IProviderRepository _providerRepository;

    public DashboardService(
        IOptimizationPlanRepository optimizationPlanRepository,
        IProviderRepository providerRepository)
    {
        _optimizationPlanRepository = optimizationPlanRepository;
        _providerRepository = providerRepository;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var totalPlans = await _optimizationPlanRepository.GetTotalCountAsync();
        var activeProviders = await _providerRepository.GetActiveProvidersCountAsync();
        var runningOptimizations = await _optimizationPlanRepository.GetRunningCountAsync();
        var completedThisMonth = await _optimizationPlanRepository.GetCompletedThisMonthCountAsync();

        return new DashboardStatsDto
        {
            TotalPlans = totalPlans,
            ActiveProviders = activeProviders,
            RunningOptimizations = runningOptimizations,
            CompletedThisMonth = completedThisMonth
        };
    }
}
