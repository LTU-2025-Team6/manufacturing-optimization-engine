namespace ManufacturingOptimization.Common.Models.DTOs;

public class ProviderDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool AutoStart { get; set; }
    public bool IsRunning { get; set; }
    public List<ProcessCapabilityDto> ProcessCapabilities { get; set; } = new();
    public TechnicalCapabilitiesDto TechnicalCapabilities { get; set; } = new();
}
