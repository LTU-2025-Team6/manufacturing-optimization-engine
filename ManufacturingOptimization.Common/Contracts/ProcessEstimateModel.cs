namespace ManufacturingOptimization.Common.Contracts;

public class ProcessEstimateModel
{
    public decimal Cost { get; set; }
    public double QualityScore { get; set; }
    public double EmissionsKgCO2 { get; set; }
    public double Duration { get; set; }
}