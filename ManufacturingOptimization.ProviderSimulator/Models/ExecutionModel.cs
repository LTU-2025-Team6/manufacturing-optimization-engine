namespace ManufacturingOptimization.ProviderSimulator.Models;

/// <summary>
/// Model representing an execution of a confirmed process proposal.
/// </summary>
public class ExecutionModel
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public Guid ExecutionScheduleId { get; set; }
    public List<ExecutionScheduleSegmentModel> ScheduleSegments { get; set; } = [];
}
