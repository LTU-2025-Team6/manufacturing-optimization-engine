namespace ManufacturingOptimization.ProviderSimulator.Data.Entities;

public class EstimateEntity
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public decimal Cost { get; set; }
    public double QualityScore { get; set; }
    public double EmissionsKgCO2 { get; set; }
    public double Duration { get; set; }

    // Navigation property
    public ProposalEntity Proposal { get; set; } = null!;
}