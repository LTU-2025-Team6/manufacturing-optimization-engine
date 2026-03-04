namespace ManufacturingOptimization.Common.Contracts
{
    public class ProviderDayScheduleModel
    {
        public DateTime Date { get; set; }
        public IReadOnlyList<ProviderScheduleSegmentModel> Segments { get; set; } = [];
    }
}