using ManufacturingOptimization.Common.Contracts;

namespace ManufacturingOptimization.Common.Enums;

/// <summary>
/// Extension methods for OptimizationPriority enum.
/// Separates business logic from domain models.
/// </summary>
public static class OptimizationPriorityExtensions
{
    /// <summary>
    /// Returns optimization weights for a given priority.
    /// Weights must sum to approximately 1.0 to ensure balanced optimization.
    /// </summary>
    /// <remarks>
    /// These weights were calibrated based on business requirements:
    /// - Primary metric gets 0.5-0.8 weight (dominant factor)
    /// - Secondary metrics get 0.05-0.2 weight (fine-tuning)
    /// - Total sum ≈ 1.0 ensures all priorities produce comparable objective values
    /// </remarks>
    public static OptimizationWeights GetWeights(this OptimizationPriority priority)
    {
        return priority switch
        {
            OptimizationPriority.LowestCost => new()
            {
                CostWeight = 0.8,      // Dominant: minimize cost
                TimeWeight = 0.1,      // Minor: prefer faster when costs equal
                QualityWeight = 0.05,  // Minimal: avoid extremely low quality
                EmissionsWeight = 0.05 // Minimal: slight preference for eco-friendly
            },
            
            OptimizationPriority.FastestDelivery => new()
            {
                CostWeight = 0.1,      // Minor: avoid excessive cost
                TimeWeight = 0.8,      // Dominant: minimize time
                QualityWeight = 0.05,  // Minimal: avoid extremely low quality
                EmissionsWeight = 0.05 // Minimal: slight preference for eco-friendly
            },
            
            OptimizationPriority.HighestQuality => new()
            {
                CostWeight = 0.2,      // Secondary: avoid excessive cost
                TimeWeight = 0.2,      // Secondary: prefer faster when quality equal
                QualityWeight = 0.5,   // Dominant: maximize quality
                EmissionsWeight = 0.1  // Minor: prefer eco-friendly
            },
            
            OptimizationPriority.LowestEmissions => new()
            {
                CostWeight = 0.1,      // Minor: avoid excessive cost
                TimeWeight = 0.1,      // Minor: prefer faster when emissions equal
                QualityWeight = 0.2,   // Secondary: maintain decent quality
                EmissionsWeight = 0.6  // Dominant: minimize emissions
            },
            
            _ => new()
            {
                CostWeight = 0.25,
                TimeWeight = 0.25,
                QualityWeight = 0.25,
                EmissionsWeight = 0.25
            }
        };
    }

    /// <summary>
    /// Returns human-readable strategy name and description for presentation layer.
    /// </summary>
    /// <remarks>
    /// This is purely presentational metadata - has no impact on optimization logic.
    /// Separated from OptimizationStep to keep solver logic clean.
    /// </remarks>
    public static (string Name, string Description) GetStrategyNameAndDescription(this OptimizationPriority priority)
    {
        return priority switch
        {
            OptimizationPriority.LowestCost => (
                "Budget Strategy",
                "Optimized for lowest total cost. Best for price-sensitive customers."
            ),

            OptimizationPriority.FastestDelivery => (
                "Express Strategy",
                "Optimized for fastest completion time. Best for urgent orders."
            ),

            OptimizationPriority.HighestQuality => (
                "Premium Strategy",
                "Optimized for highest quality and reliability. Best long-term value."
            ),

            OptimizationPriority.LowestEmissions => (
                "Eco Strategy",
                "Optimized for minimal carbon emissions. Best for sustainability goals."
            ),

            _ => (
                "Balanced Strategy",
                "Balanced optimization across cost, time, quality, and emissions."
            )
        };
    }

    /// <summary>
    /// Determines warranty terms and insurance based on optimization priority and workflow type.
    /// </summary>
    /// <remarks>
    /// Business rules (not optimization logic):
    /// - Premium priorities (Quality, Eco) get better warranty terms
    /// - Budget priority gets minimal warranty (cost savings)
    /// - Upgrade workflows receive enhanced warranty terms
    /// - Insurance bundled with higher-tier warranties
    /// </remarks>
    public static WarrantyTermsModel GetWarrantyTerms(this OptimizationPriority priority, string workflowType)
    {
        var isUpgrade = workflowType.Equals("Upgrade", StringComparison.OrdinalIgnoreCase);

        return priority switch
        {
            OptimizationPriority.HighestQuality => isUpgrade
                ? new() { Level = "Platinum", DurationMonths = 36, Description = "Platinum 3 Years", IncludesInsurance = true }
                : new() { Level = "Gold", DurationMonths = 18, Description = "Gold 18 Months", IncludesInsurance = true },

            OptimizationPriority.FastestDelivery => isUpgrade
                ? new() { Level = "Gold", DurationMonths = 12, Description = "Gold 12 Months", IncludesInsurance = true }
                : new() { Level = "Silver", DurationMonths = 6, Description = "Silver 6 Months", IncludesInsurance = false },

            OptimizationPriority.LowestEmissions => isUpgrade
                ? new() { Level = "Gold", DurationMonths = 12, Description = "Gold 12 Months", IncludesInsurance = true }
                : new() { Level = "Silver", DurationMonths = 9, Description = "Silver 9 Months", IncludesInsurance = true },

            OptimizationPriority.LowestCost => new()
            {
                Level = "Basic",
                DurationMonths = 3,
                Description = "Basic 3 Months",
                IncludesInsurance = false
            },

            _ => new()
            {
                Level = "Standard",
                DurationMonths = 6,
                Description = "Standard 6 Months",
                IncludesInsurance = false
            }
        };
    }
}
