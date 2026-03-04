namespace ManufacturingOptimization.Gateway.Data.Entities;

/// <summary>
/// Technical capabilities entity for database storage.
/// </summary>
public class TechnicalCapabilitiesEntity
{
    public Guid Id { get; set; }
    public Guid ProviderId { get; set; }
    public double AxisHeight { get; set; }
    public double Power { get; set; }
    public double Tolerance { get; set; }

    // Navigation property
    public ProviderEntity Provider { get; set; } = null!;
}
