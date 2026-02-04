namespace ManufacturingOptimization.Common.Models.DTOs;

public class ProcessEstimateDto
{
    public Guid Id { get; set; }
    public decimal Cost { get; set; }
    public double QualityScore { get; set; }
    public double EmissionsKgCO2 { get; set; }
    public double Duration { get; set; }
    public List<TimeWindowDto> AvailableTimeSlots { get; set; } = new();
}