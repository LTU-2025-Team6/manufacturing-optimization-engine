namespace ManufacturingOptimization.ProviderSimulator.Data.Entities;

public class ExecutionEntity
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }

    // Navigation properties
    public ProposalEntity Proposal { get; set; } = null!;
    public List<ExecutionScheduleSegmentEntity> ScheduleSegments { get; set; } = null!;
}