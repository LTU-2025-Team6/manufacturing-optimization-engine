using ManufacturingOptimization.Common.Enums;

namespace ManufacturingOptimization.Common.Contracts;

/// <summary>
/// Detailed information about an execution for UI display.
/// </summary>
public class ExecutionDetailsModel
{
    public Guid ExecutionId { get; set; }
    public Guid ProposalId { get; set; }
    public Guid PlanId { get; set; }
    public Guid ProviderId { get; set; }
    public ProcessType Process { get; set; }
    public ProposalStatus Status { get; set; }
    
    // Timestamps
    public DateTime ArrivedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    
    // Motor specifications for the process
    public MotorSpecificationsModel MotorSpecs { get; set; } = null!;
    
    // Process estimate
    public ProcessEstimateModel? Estimate { get; set; }
    
    // Execution schedule segments (working time slots)
    public List<ExecutionTimeSlot> ScheduleSegments { get; set; } = [];
}

/// <summary>
/// Time segment within an execution schedule.
/// </summary>
public class ExecutionTimeSlot
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
