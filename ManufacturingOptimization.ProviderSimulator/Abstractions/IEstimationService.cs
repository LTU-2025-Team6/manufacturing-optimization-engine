using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.ProviderSimulator.Abstractions;

public interface IEstimationService
{
    double CalculateComplexityFactor(MotorSpecificationsModel motorSpecs, ProcessType process);
    double CalculateDuration(ProcessType process, ProcessCapabilityModel capability, MotorSpecificationsModel motorSpecs, double complexity);
    decimal CalculateCost(ProcessCapabilityModel capability, double duration, MotorSpecificationsModel motorSpecs, double complexity);
    double CalculateQualityScore(ProcessCapabilityModel capability, MotorSpecificationsModel motorSpecs, ProcessType process);
    double CalculateEmissions(ProcessCapabilityModel capability, double duration, MotorSpecificationsModel motorSpecs);
}
