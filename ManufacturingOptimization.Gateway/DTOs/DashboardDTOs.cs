namespace ManufacturingOptimization.Gateway.DTOs;

public class DashboardStatsDto
{
    public int TotalPlans { get; set; }
    public int ActiveProviders { get; set; }
    public int RunningOptimizations { get; set; }    public int CompletedThisMonth { get; set; }
}
