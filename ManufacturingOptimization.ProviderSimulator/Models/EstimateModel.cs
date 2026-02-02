namespace ManufacturingOptimization.ProviderSimulator.Models;

/// <summary>
/// Model representing a process estimate with cost, quality, and scheduling information.
/// </summary>
public class EstimateModel
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public decimal Cost { get; set; }
    public double QualityScore { get; set; }
    public double EmissionsKgCO2 { get; set; }
    public double Duration { get; set; }
}
