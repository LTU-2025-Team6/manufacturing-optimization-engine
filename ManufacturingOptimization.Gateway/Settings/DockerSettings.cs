namespace ManufacturingOptimization.Gateway.Settings;

/// <summary>
/// Docker container settings for provider orchestration.
/// Values should be provided via environment variables (Docker__ProviderImage, Docker__Network).
/// </summary>
public class DockerSettings
{
    public const string SectionName = "Docker";

    public string ProviderImage { get; set; } = string.Empty;
    public string Network { get; set; } = string.Empty;
}
