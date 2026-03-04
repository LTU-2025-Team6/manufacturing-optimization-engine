namespace ManufacturingOptimization.Gateway.DTOs.Dashboard;

/// <summary>
/// Dashboard statistics.
/// </summary>
public class DashboardStatsDto
{
    public int TotalPlans { get; set; }
    public int ActiveProviders { get; set; }
    public int RunningOptimizations { get; set; }
    public int CompletedThisMonth { get; set; }
}
