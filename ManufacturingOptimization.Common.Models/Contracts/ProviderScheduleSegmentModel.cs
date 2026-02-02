using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Common.Models.Contracts;

public class ProviderScheduleSegmentModel
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public SegmentType SegmentType { get; set; } = SegmentType.FreeSpace;
}
