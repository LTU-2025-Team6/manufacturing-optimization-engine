using ManufacturingOptimization.Common.Enums;

namespace ManufacturingOptimization.ProviderSimulator.Data.Entities;

public class ExecutionEntity
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    
    /// <summary>
    /// Current execution status (Pending, InProgress, Completed, Failed).
    /// </summary>
    public StepExecutionStatus Status { get; set; } = StepExecutionStatus.Pending;
    
    /// <summary>
    /// Indicates if this execution was auto-generated for demo purposes.
    /// Demo executions are ignored by the ExecutionSchedulerService.
    /// </summary>
    public bool IsDemo { get; set; } = false;
    
    /// <summary>
    /// When execution actually started (simulation time).
    /// </summary>
    public DateTime? StartedAt { get; set; }
    
    /// <summary>
    /// When execution completed or failed (simulation time).
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    public ProposalEntity Proposal { get; set; } = null!;
    public List<ExecutionScheduleSegmentEntity> ScheduleSegments { get; set; } = null!;
}