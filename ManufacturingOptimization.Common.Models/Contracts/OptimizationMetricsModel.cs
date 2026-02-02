namespace ManufacturingOptimization.Common.Models.Contracts;

public class OptimizationMetricsModel
{
    public Guid Id { get; set; }
    public decimal TotalCost { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public double AverageQuality { get; set; }
    public double TotalEmissionsKgCO2 { get; set; }
    public string SolverStatus { get; set; } = string.Empty;
    public double ObjectiveValue { get; set; }
}
