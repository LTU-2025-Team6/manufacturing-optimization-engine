using ManufacturingOptimization.Common.Contracts;
using ManufacturingOptimization.ProviderSimulator.Models;

namespace ManufacturingOptimization.ProviderSimulator.Settings;

/// <summary>
/// Settings for Main Remanufacturing Center provider (TP1).
/// Configure via environment variables: Provider__ProviderId, Provider__ProviderName
/// </summary>
public class ProviderSettings
{
    public const string SectionName = "Provider";
    
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string EnvironmentSource { get; set; } = string.Empty;

    public List<ProcessCapabilityModel> ProcessCapabilities { get; set; } = new();
    public TechnicalCapabilitiesModel TechnicalCapabilities { get; set; } = new();
    public ProviderWorkingHoursModel WorkingHours { get; set; } = new();
}
