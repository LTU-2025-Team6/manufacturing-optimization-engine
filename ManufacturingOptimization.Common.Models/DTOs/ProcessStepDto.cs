namespace ManufacturingOptimization.Common.Models.DTOs;

public class ProcessStepDto
{
    public Guid Id { get; set; }
    public int StepNumber { get; set; }
    public string Process { get; set; } = string.Empty;
    public Guid SelectedProviderId { get; set; }
    public string SelectedProviderName { get; set; } = string.Empty;
    public ProcessEstimateDto Estimate { get; set; } = new();
    public ProviderScheduleDto? AllocatedSchedule { get; set; }
}
