namespace ManufacturingOptimization.Gateway.DTOs;

public class ProviderScheduleDto
{
    public DateTime StartWorkingTime { get; set; }
    public DateTime EndWorkingTime { get; set; }
    public List<ProviderScheduleSegmentDto> Segments { get; set; } = [];
}

public class ProviderDayScheduleDto
{
    public DateTime Date { get; set; }
    public List<ProviderScheduleSegmentDto> Segments { get; set; } = [];
}

public class ProviderScheduleSegmentDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string SegmentType { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the execution if this segment represents an occupied time slot.
    /// Null for FreeSpace and Break segments.
    /// </summary>
    public Guid? ExecutionId { get; set; }
}