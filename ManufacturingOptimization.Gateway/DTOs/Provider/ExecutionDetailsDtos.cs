using ManufacturingOptimization.Gateway.DTOs.OptimizationRequest;

namespace ManufacturingOptimization.Gateway.DTOs.Provider;

/// <summary>
/// Execution details from provider.
/// </summary>
public class ExecutionDetailsDto
{  
    public Guid ExecutionId { get; set; }
    public Guid ProposalId { get; set; }
    public Guid PlanId { get; set; }
    public Guid ProviderId { get; set; }
    public string Process { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ArrivedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public MotorSpecificationsDto MotorSpecs { get; set; } = new();
    public ProcessEstimateDto? Estimate { get; set; }
    public List<ExecutionScheduleSegmentDto> ScheduleSegments { get; set; } = [];
}

public class ExecutionScheduleSegmentDto
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}
