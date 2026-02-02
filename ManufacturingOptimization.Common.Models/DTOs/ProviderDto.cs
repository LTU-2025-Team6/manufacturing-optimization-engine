namespace ManufacturingOptimization.Common.Models.DTOs;

public class ProviderDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public List<ProcessCapabilityDto> ProcessCapabilities { get; set; } = new();
    public TechnicalCapabilitiesDto TechnicalCapabilities { get; set; } = new();
}
