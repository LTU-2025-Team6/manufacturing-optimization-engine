namespace ManufacturingOptimization.ProviderSimulator.Data.Entities;

public class ExecutionEntity
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }

    public ProposalEntity Proposal { get; set; } = null!;
    public List<ExecutionScheduleSegmentEntity> ScheduleSegments { get; set; } = null!;
}