namespace ManufacturingOptimization.Common.Models.DTOs;

public class ProcessCapabilityDto
{
    public Guid Id { get; set; }
    public string Process { get; set; } = string.Empty;
    public decimal CostPerHour { get; set; }
    public double SpeedMultiplier { get; set; }
    public double QualityScore { get; set; }
    public double EnergyConsumptionKwhPerHour { get; set; }
    public double CarbonIntensityKgCO2PerKwh { get; set; }
    public bool UsesRenewableEnergy { get; set; }
}
