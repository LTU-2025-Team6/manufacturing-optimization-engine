namespace ManufacturingOptimization.Common.Models.DTOs;

public class ProviderScheduleSegmentDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string SegmentType { get; set; } = string.Empty;
    public int SegmentOrder { get; set; }
}
