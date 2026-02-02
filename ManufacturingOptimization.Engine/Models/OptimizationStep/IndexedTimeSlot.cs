using ManufacturingOptimization.Common.Models.Contracts;

namespace ManufacturingOptimization.Engine.Models.OptimizationStep;

/// <summary>
/// Represents an indexed time slot with hours calculated relative to a reference time.
/// Used for MIP optimization to convert DateTime slots into numeric hours.
/// </summary>
public class IndexedTimeSlot
{
    /// <summary>
    /// Index of this slot in the provider's available slots list.
    /// </summary>
    public int SlotIndex { get; set; }
    
    /// <summary>
    /// The actual time window (original slot with DateTime values).
    /// </summary>
    public ProviderScheduleModel Slot { get; set; } = null!;

    /// <summary>
    /// Start time in hours relative to the reference time (RequestedWindow.StartTime).
    /// </summary>
    public double StartTimeHours { get; set; }
    
    /// <summary>
    /// End time in hours relative to the reference time (RequestedWindow.StartTime).
    /// </summary>
    public double EndTimeHours { get; set; }
}
