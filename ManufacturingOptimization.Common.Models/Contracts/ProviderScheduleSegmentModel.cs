using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Common.Models.Contracts;

public class ProviderScheduleSegmentModel
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public SegmentType SegmentType { get; set; } = SegmentType.FreeSpace;
    
    /// <summary>
    /// ExecutionId for Occupied segments. Null for FreeSpace and Break segments.
    /// </summary>
    public Guid? ExecutionId { get; set; }
}
