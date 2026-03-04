namespace ManufacturingOptimization.Gateway.Data.Entities;

/// <summary>
/// Database entity for a time segment within a process step execution.
/// </summary>
public class ProviderScheduleSegmentEntity
{
    public Guid Id { get; set; }
    public Guid ProviderScheduleId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string SegmentType { get; set; } = string.Empty;

    // Navigation property
    public ProviderScheduleEntity ProviderSchedule { get; set; } = null!;
}
