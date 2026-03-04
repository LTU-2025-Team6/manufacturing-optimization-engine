namespace ManufacturingOptimization.Gateway.Data.Entities;

/// <summary>
/// Warranty terms entity for database storage.
/// </summary>
public class WarrantyTermsEntity
{
    public Guid Id { get; set; }
    public Guid StrategyId { get; set; }
    public string Level { get; set; } = string.Empty;
    public int DurationMonths { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IncludesInsurance { get; set; }

    // Navigation property
    public OptimizationStrategyEntity Strategy { get; set; } = null!;
}
