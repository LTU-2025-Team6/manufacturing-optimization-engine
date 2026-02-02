using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.ProviderSimulator.Models;

public class ProposalModel
{
    public Guid Id { get; set; }
    public Guid PlanId { get; set; }
    public Guid ProviderId { get; set; }
    public ProcessType Process { get; set; }
    public ProposalStatus Status { get; set; }
    public string? DeclineReason { get; set; }
    public DateTime ArrivedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public MotorSpecificationsModel MotorSpecs { get; set; } = null!;
    public EstimateModel? Estimate { get; set; }
    public ExecutionModel? Execution { get; set; }
}
