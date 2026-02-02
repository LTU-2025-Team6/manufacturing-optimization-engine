namespace ManufacturingOptimization.Common.Models.DTOs
{
    public class OptimizationRequestDto
    {
        public string CustomerId { get; set; } = string.Empty;
        public MotorSpecificationsDto MotorSpecs { get; set; } = null!;
        public OptimizationRequestConstraintsDto Constraints { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}