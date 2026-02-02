namespace ManufacturingOptimization.Common.Models.DTOs
{
    public class MotorSpecificationsDto
    {
        public double PowerKW { get; set; }
        public int AxisHeightMM { get; set; }
        public string CurrentEfficiency { get; set; } = string.Empty;
        public string TargetEfficiency { get; set; } = string.Empty;
        public string? MalfunctionDescription { get; set; }
    }
}