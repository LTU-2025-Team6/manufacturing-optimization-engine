using ManufacturingOptimization.Common.Enums;
using System.Text.Json.Serialization;

namespace ManufacturingOptimization.Common.Contracts;

public class ProcessCapabilityModel
{
    public Guid Id { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ProcessType Process { get; set; }
    public decimal CostPerHour { get; set; }
    public double SpeedMultiplier { get; set; } = 1.0;
    public double QualityScore { get; set; } = 0.8;
    public double EnergyConsumptionKwhPerHour { get; set; }
    public double CarbonIntensityKgCO2PerKwh { get; set; } = 0.5;
    public bool UsesRenewableEnergy { get; set; }
}
