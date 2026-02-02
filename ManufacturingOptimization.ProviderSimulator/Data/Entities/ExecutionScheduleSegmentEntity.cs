namespace ManufacturingOptimization.ProviderSimulator.Data.Entities;

public class ExecutionScheduleSegmentEntity
{
    public Guid Id { get; set; }
    public Guid ExecutionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    // Navigation property
    public ExecutionEntity Execution { get; set; } = null!;
}