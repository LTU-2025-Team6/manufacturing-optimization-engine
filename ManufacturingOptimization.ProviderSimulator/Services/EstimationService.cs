using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Models;

namespace ManufacturingOptimization.ProviderSimulator.Services;

/// <summary>
/// Service for estimation calculations.
/// Shared logic between ProcessProposalHandler and DemoDataGenerator.
/// </summary>
public class EstimationService : IEstimationService
{
    private readonly IProviderSimulationContext _context;

    public EstimationService(IProviderSimulationContext context)
    {
        _context = context;
    }

    public double CalculateComplexityFactor(MotorSpecificationsModel motorSpecs, ProcessType process)
    {
        double complexity = 1.0;

        // Larger motors are more complex to handle
        if (motorSpecs.PowerKW > 100)
            complexity += 0.3;
        else if (motorSpecs.PowerKW > 50)
            complexity += 0.15;

        // Larger axis height increases complexity
        if (motorSpecs.AxisHeightMM > 315)
            complexity += 0.25;
        else if (motorSpecs.AxisHeightMM > 200)
            complexity += 0.1;

        // Efficiency upgrade complexity
        var efficiencyGap = (int)motorSpecs.TargetEfficiency - (int)motorSpecs.CurrentEfficiency;
        if (efficiencyGap > 2)
            complexity += 0.4;
        else if (efficiencyGap > 1)
            complexity += 0.2;
        else if (efficiencyGap > 0)
            complexity += 0.1;

        // Malfunction adds complexity for various processes
        if (!string.IsNullOrWhiteSpace(motorSpecs.MalfunctionDescription))
        {
            if (process == ProcessType.Disassembly || process == ProcessType.PartSubstitution)
                complexity += 0.5; // Major impact on disassembly and repair
            else if (process == ProcessType.Certification)
                complexity += 0.3; // Moderate impact on testing
            else if (process == ProcessType.Cleaning)
                complexity += 0.2; // Some impact on cleaning damaged parts
        }

        return complexity;
    }

    public double CalculateDuration(ProcessType process, ProcessCapabilityModel capability, MotorSpecificationsModel motorSpecs, double complexity)
    {
        var baseDuration = _context.StandardDurations
            .GetValueOrDefault(process, 8.0);

        var duration = baseDuration * capability.SpeedMultiplier * complexity;

        // Additional time for high-power motors in certain processes
        if (motorSpecs.PowerKW > 150)
        {
            if (process == ProcessType.Certification || process == ProcessType.Disassembly || process == ProcessType.Reassembly)
                duration += 2.0; // Extra 2 hours for large motor handling
            else if (process == ProcessType.Turning || process == ProcessType.Grinding)
                duration += 1.5; // Extra time for precision work on large components
        }

        return duration;
    }

    public decimal CalculateCost(ProcessCapabilityModel capability, double duration, MotorSpecificationsModel motorSpecs, double complexity)
    {
        var baseCost = capability.CostPerHour * (decimal)duration;

        // Premium for high-power motors (specialized equipment needed)
        if (motorSpecs.PowerKW > 100)
            baseCost *= 1.15m;

        // Premium for large motors (handling equipment)
        if (motorSpecs.AxisHeightMM > 280)
            baseCost *= 1.1m;

        // Additional cost for efficiency upgrade materials
        var efficiencyGap = (int)motorSpecs.TargetEfficiency - (int)motorSpecs.CurrentEfficiency;
        if (efficiencyGap > 0)
            baseCost += efficiencyGap * 500m; // Material costs per efficiency class

        return baseCost;
    }

    public double CalculateQualityScore(ProcessCapabilityModel capability, MotorSpecificationsModel motorSpecs, ProcessType process)
    {
        var baseQuality = capability.QualityScore;

        // Quality may vary based on motor size - larger motors are harder to work with
        if (motorSpecs.AxisHeightMM > 315)
            baseQuality -= 0.05; // Slightly lower quality for very large motors
        else if (motorSpecs.AxisHeightMM < 132)
            baseQuality += 0.03; // Easier to achieve high quality on smaller motors

        if (process == ProcessType.PartSubstitution || process == ProcessType.Reassembly)
            baseQuality -= 0.08; // Unknown issues may affect final quality
        else if (process == ProcessType.Certification)
            baseQuality -= 0.05; // May affect test results

        // Large efficiency jumps may be harder to guarantee
        var efficiencyGap = (int)motorSpecs.TargetEfficiency - (int)motorSpecs.CurrentEfficiency;
        if (efficiencyGap > 2)
            baseQuality -= 0.1;

        // Precision processes benefit from smaller motors
        if (process == ProcessType.Grinding || process == ProcessType.Turning)
        {
            if (motorSpecs.AxisHeightMM < 160)
                baseQuality += 0.05;
        }

        // Clamp between 0 and 1
        return Math.Max(0.0, Math.Min(1.0, baseQuality));
    }

    public double CalculateEmissions(ProcessCapabilityModel capability, double duration, MotorSpecificationsModel motorSpecs)
    {
        var baseEmissions = capability.EnergyConsumptionKwhPerHour
             * capability.CarbonIntensityKgCO2PerKwh
             * duration;

        // Higher power motors require more energy to process (testing, handling equipment)
        var powerFactor = 1.0 + (motorSpecs.PowerKW / 1000.0); // Small increase based on motor power

        return baseEmissions * powerFactor;
    }
}
