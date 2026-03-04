using ManufacturingOptimization.Common.Enums;

namespace ManufacturingOptimization.Common.Contracts
{
    public class ProviderScheduleModel
    {
        public Guid Id { get; set; }
        
        public DateTime StartWorkingTime
        {
            get
            {
                var workingSegments = Segments.Where(s => s.SegmentType == SegmentType.WorkingTime).ToList();
                return workingSegments.Count > 0 ? workingSegments.Min(s => s.StartTime) : DateTime.MinValue;
            }
        }

        public DateTime EndWorkingTime
        {
            get
            {
                var workingSegments = Segments.Where(s => s.SegmentType == SegmentType.WorkingTime).ToList();
                return workingSegments.Count > 0 ? workingSegments.Max(s => s.EndTime) : DateTime.MinValue;
            }
        }
        
        public IReadOnlyList<ProviderScheduleSegmentModel> Segments { get; set; } = [];
    }
}