namespace ManufacturingOptimization.Common.Models.Contracts
{
    public class ProviderScheduleModel
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public IReadOnlyList<ProviderScheduleSegmentModel> Segments { get; set; } = [];
    }
}