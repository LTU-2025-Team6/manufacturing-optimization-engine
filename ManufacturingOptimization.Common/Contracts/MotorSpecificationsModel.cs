using ManufacturingOptimization.Common.Enums;

namespace ManufacturingOptimization.Common.Contracts
{
    public class MotorSpecificationsModel
    {
        public double PowerKW { get; set; }
        public int AxisHeightMM { get; set; }
        public MotorEfficiencyClass CurrentEfficiency { get; set; }
        public MotorEfficiencyClass TargetEfficiency { get; set; }
        public string? MalfunctionDescription { get; set; }
    }
}