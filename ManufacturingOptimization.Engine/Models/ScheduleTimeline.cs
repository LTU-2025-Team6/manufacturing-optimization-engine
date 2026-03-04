namespace ManufacturingOptimization.Engine.Models;

/// <summary>
/// Complete timeline of scheduled processes.
/// Contains the ordered sequence of when each process will be executed.
/// </summary>
public class ScheduleTimeline
{
    /// <summary>
    /// Reference time for the schedule (typically RequestedWindow.StartTime).
    /// All times are calculated relative to this.
    /// </summary>
    public DateTime ReferenceTime { get; set; }
    
    /// <summary>
    /// Ordered list of scheduled processes.
    /// </summary>
    public List<ScheduledProcess> Processes { get; set; } = new();
}
