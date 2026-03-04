namespace ManufacturingOptimization.Gateway.Data.Entities;

/// <summary>
/// Provider entity for database storage across all services.
/// </summary>
public class ProviderEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool AutoStart { get; set; }
    public bool IsRunning { get; set; }
    public string EnvironmentSource { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<ProcessCapabilityEntity> ProcessCapabilities { get; set; } = new List<ProcessCapabilityEntity>();
    public TechnicalCapabilitiesEntity? TechnicalCapabilities { get; set; }
    public ProviderWorkingHoursEntity? WorkingHours { get; set; }
}
