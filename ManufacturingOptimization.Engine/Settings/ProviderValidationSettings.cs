using ManufacturingOptimization.Common.Enums;
using System.Text.Json.Serialization;

namespace ManufacturingOptimization.Engine.Settings;

public class ProviderValidationSettings
{
    public const string SectionName = "ProviderValidation";

    public TechnicalLimits TechnicalLimits { get; set; } = new();
    public RequiredCapabilities RequiredCapabilities { get; set; } = new();

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public List<ProcessType> RequiredProcesses { get; set; } = new();
}

public class TechnicalLimits
{
    public double MinAxisHeight { get; set; }
    public double MaxAxisHeight { get; set; }
    public double MinPower { get; set; }
    public double MaxPower { get; set; }
    public double MinTolerance { get; set; }
    public double MaxTolerance { get; set; }
}

public class RequiredCapabilities
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public List<ProcessType> MainRemanufacturingCenter { get; set; } = new();

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public List<ProcessType> EngineeringDesignFirm { get; set; } = new();

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public List<ProcessType> PrecisionMachineShop { get; set; } = new();
}
