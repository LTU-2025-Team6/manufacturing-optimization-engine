namespace ManufacturingOptimization.ProviderSimulator.Data.Entities;

/// <summary>
/// Entity to track if demo data has been generated for each provider.
/// Each provider instance has its own record to prevent duplicate generation.
/// </summary>
public class DemoDataStatusEntity
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Unique identifier of the provider (stored as string)
    /// </summary>
    public string ProviderId { get; set; } = null!;
    
    /// <summary>
    /// Indicates if demo data has been generated for this provider
    /// </summary>
    public bool IsGenerated { get; set; }
    
    /// <summary>
    /// Timestamp when demo data was generated
    /// </summary>
    public DateTime? GeneratedAt { get; set; }
}
