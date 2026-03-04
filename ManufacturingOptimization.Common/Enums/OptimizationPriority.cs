namespace ManufacturingOptimization.Common.Enums
{
    /// <summary>
    /// Optimization priority for strategy generation.
    /// Each priority generates a different optimization strategy.
    /// </summary>
    public enum OptimizationPriority
    {
        /// <summary>
        /// Minimize total cost - Budget Strategy
        /// </summary>
        LowestCost,
        
        /// <summary>
        /// Minimize total time - Express Strategy
        /// </summary>
        FastestDelivery,
        
        /// <summary>
        /// Maximize quality - Premium Strategy
        /// </summary>
        HighestQuality,
        
        /// <summary>
        /// Minimize emissions (kg CO2) - Eco Strategy
        /// </summary>
        LowestEmissions
    }
}