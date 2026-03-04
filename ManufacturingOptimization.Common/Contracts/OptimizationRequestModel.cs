namespace ManufacturingOptimization.Common.Contracts
{
    public class OptimizationRequestModel
    {
        public Guid RequestId { get; set; } = Guid.NewGuid();
        public string CustomerId { get; set; } = string.Empty;
        public MotorSpecificationsModel MotorSpecs { get; set; } = null!;
        public OptimizationRequestConstraintsModel Constraints { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}