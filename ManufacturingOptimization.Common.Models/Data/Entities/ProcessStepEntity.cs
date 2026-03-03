namespace ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Common.Models.Enums;

/// <summary>
/// Process step entity for database storage.
/// </summary>
public class ProcessStepEntity
{
    public Guid Id { get; set; }
    public Guid StrategyId { get; set; }
    public int StepNumber { get; set; }
    public string Process { get; set; } = string.Empty;
    public Guid ProposalId { get; set; }
    public Guid SelectedProviderId { get; set; }
    public string SelectedProviderName { get; set; } = string.Empty;
    public Guid? ProviderScheduleId { get; set; }

    // Navigation properties
    public OptimizationStrategyEntity Strategy { get; set; } = null!;
    public ProcessEstimateEntity? Estimate { get; set; }
    public ProviderScheduleEntity? ProviderSchedule { get; set; }
    public StepExecutionStatus ExecutionStatus { get; set; } = StepExecutionStatus.Pending;
}
