using ManufacturingOptimization.ProviderSimulator.Data.Entities;

namespace ManufacturingOptimization.ProviderSimulator.Models;

/// <summary>
/// Model representing a time segment within an execution schedule.
/// </summary>
public class ExecutionScheduleSegmentModel
{
    public Guid Id { get; set; }
    public Guid ExecutionId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public ExecutionEntity Execution { get; set; } = null!;
}
