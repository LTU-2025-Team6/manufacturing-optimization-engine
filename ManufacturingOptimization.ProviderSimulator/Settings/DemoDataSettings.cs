namespace ManufacturingOptimization.ProviderSimulator.Settings;

/// <summary>
/// Settings for demo data generation.
/// </summary>
public class DemoDataSettings
{
    public const string SectionName = "DemoData";

    public bool Enabled { get; set; } = false;
    public int MonthsAhead { get; set; } = 4;
    public int ExecutionsPerMonth { get; set; } = 15;
    public int? RandomSeed { get; set; }
}
