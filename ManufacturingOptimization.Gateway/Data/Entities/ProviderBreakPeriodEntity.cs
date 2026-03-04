namespace ManufacturingOptimization.Gateway.Data.Entities;

/// <summary>
/// Entity representing a break period during working hours.
/// This is owned by ProviderWorkingHoursEntity.
/// </summary>
public class ProviderBreakPeriodEntity
{
    public int Id { get; set; }
    public Guid ProviderId { get; set; }
    public int StartHour { get; set; }
    public int StartMinute { get; set; }
    public int DurationMinutes { get; set; }
    public string Name { get; set; } = "Break";
    
    // Navigation
    public ProviderWorkingHoursEntity WorkingHours { get; set; } = null!;
}
