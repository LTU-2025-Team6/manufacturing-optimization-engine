using ManufacturingOptimization.Common.Enums;

namespace ManufacturingOptimization.Common.Contracts;

public class ProcessStepModel
{
    public Guid Id { get; set; } = Guid.NewGuid(); // used in execution pipeline
    public int StepNumber { get; set; }
    public ProcessType Process { get; set; }
    public Guid SelectedProviderId { get; set; }
    public string SelectedProviderName { get; set; } = string.Empty;
    public Guid ProposalId { get; set; }
    public ProcessEstimateModel Estimate { get; set; } = new();
    public ProviderScheduleModel? AllocatedSchedule { get; set; }
}