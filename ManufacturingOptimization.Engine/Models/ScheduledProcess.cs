using ManufacturingOptimization.Common.Contracts;
using ManufacturingOptimization.Common.Enums;

namespace ManufacturingOptimization.Engine.Models;

/// <summary>
/// Represents a scheduled process with specific start/end times.
/// Result of MIP optimization that allocates processes to time slots.
/// </summary>
public class ScheduledProcess
{
    /// <summary>
    /// Sequential step number in the workflow.
    /// </summary>
    public int StepNumber { get; set; }
    
    /// <summary>
    /// Type of process being scheduled.
    /// </summary>
    public ProcessType Process { get; set; }
    
    /// <summary>
    /// ID of the provider selected to execute this process.
    /// </summary>
    public Guid ProviderId { get; set; }
    
    /// <summary>
    /// Name of the provider.
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;
    
    /// <summary>
    /// The allocated time slot for this process (single period accounting for breaks).
    /// </summary>
    public ProviderScheduleModel? AllocatedSchedule { get; set; }
}
