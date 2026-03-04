namespace ManufacturingOptimization.Gateway.Data.Entities;

/// <summary>
/// Process capability entity for database storage.
/// </summary>
public class ProcessCapabilityEntity
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public string Process { get; set; } = string.Empty;
    public double CostPerHour { get; set; }
    public double SpeedMultiplier { get; set; }
    public double QualityScore { get; set; }
    public double EnergyConsumptionKwhPerHour { get; set; }
    public double CarbonIntensityKgCO2PerKwh { get; set; }
    public bool UsesRenewableEnergy { get; set; }

    // Navigation property
    public ProviderEntity Provider { get; set; } = null!;
}
