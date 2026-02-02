using ManufacturingOptimization.Common.Models.Contracts;

namespace ManufacturingOptimization.Engine.Models.OptimizationStep;

/// <summary>
/// Provider matched to a specific process step.
/// </summary>
public class MatchedProvider
{
    public required Guid ProviderId { get; init; }
    public string ProviderName { get; set; } = string.Empty;
    public Guid ProposalId { get; set; }
    public ProcessEstimateModel Estimate { get; set; } = null!;
    public ProviderScheduleModel Schedule { get; set; } = null!;
    public List<IndexedTimeSlot> IndexedSlots { get; set; } = null!;
}
