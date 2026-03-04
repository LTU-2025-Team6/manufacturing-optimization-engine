using ManufacturingOptimization.Common.Enums;

namespace ManufacturingOptimization.ProviderSimulator.Data.Entities;

public class ProposalEntity
{
    public Guid Id { get; set; }
    public Guid PlanId { get; set; }
    public Guid ProviderId { get; set; }
    public ProcessType Process { get; set; }
    public ProposalStatus Status { get; set; }
    public string? DeclineReason { get; set; }
    public DateTime ArrivedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public MotorSpecificationsEntity MotorSpecs { get; set; } = new();

    // Navigation properties
    public EstimateEntity? Estimate { get; set; }
    public ExecutionEntity? Execution { get; set; }
}