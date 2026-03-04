namespace ManufacturingOptimization.Common.Contracts
{
    public class OptimizationRequestConstraintsModel
    {
        public decimal? MaxBudget { get; set; }
        public TimeWindowModel TimeWindow { get; set; } = null!;
    }
}