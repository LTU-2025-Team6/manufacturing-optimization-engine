namespace ManufacturingOptimization.Gateway.Settings;

/// <summary>
/// Settings for provider orchestration mode.
/// Value should be provided via environment variable (Orchestration__Mode).
/// </summary>
public class OrchestrationSettings
{
    public const string SectionName = "Orchestration";
    
    public string Mode { get; set; } = string.Empty;

    public bool IsProductionMode => Mode.Equals("Production", StringComparison.OrdinalIgnoreCase);
    public bool IsDevelopmentMode => Mode.Equals("Development", StringComparison.OrdinalIgnoreCase);
}
