namespace ManufacturingOptimization.Common.Models.Contracts;

public class ProviderModel
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public List<ProcessCapabilityModel> ProcessCapabilities { get; set; } = [];
    public TechnicalCapabilitiesModel TechnicalCapabilities { get; set; } = null!;
    public ProviderWorkingHoursModel WorkingHours { get; set; } = null!;
}
