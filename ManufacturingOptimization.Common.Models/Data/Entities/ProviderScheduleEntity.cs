namespace ManufacturingOptimization.Common.Models.Data.Entities;

/// <summary>
/// Database entity for an allocated time slot with detailed segment breakdown.
/// </summary>
public class ProviderScheduleEntity
{
    public Guid Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    // Navigation properties
    public List<ProviderScheduleSegmentEntity> Segments { get; set; } = new();
}
