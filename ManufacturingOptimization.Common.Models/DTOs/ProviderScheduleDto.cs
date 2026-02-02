namespace ManufacturingOptimization.Common.Models.DTOs
{
    public class ProviderScheduleDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<ProviderScheduleSegmentDto> Segments { get; set; } = [];
    }
}
