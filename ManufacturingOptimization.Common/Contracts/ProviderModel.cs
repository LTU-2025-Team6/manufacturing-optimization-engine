namespace ManufacturingOptimization.Common.Contracts;

public class ProviderModel
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public  bool AutoStart { get; set; }
    public bool IsRunning { get; set; }
    public string EnvironmentSource { get; set; } = string.Empty;
    public List<ProcessCapabilityModel> ProcessCapabilities { get; set; } = [];
    public TechnicalCapabilitiesModel TechnicalCapabilities { get; set; } = null!;
    public ProviderWorkingHoursModel WorkingHours { get; set; } = null!;
}
