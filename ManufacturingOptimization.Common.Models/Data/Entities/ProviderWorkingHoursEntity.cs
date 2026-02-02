namespace ManufacturingOptimization.Common.Models.Data.Entities;

/// <summary>
/// Entity representing working hours configuration for a provider.
/// This is owned by ProviderEntity.
/// </summary>
public class ProviderWorkingHoursEntity
{
    public Guid ProviderId { get; set; }
    public int WorkDayStartHour { get; set; }
    public int WorkDayEndHour { get; set; }
    public bool Is24x7 { get; set; }
    public string WorkingDaysJson { get; set; } = string.Empty; // Serialized HashSet<DayOfWeek>
    
    // Navigation
    public ProviderEntity Provider { get; set; } = null!;
    public ICollection<ProviderBreakPeriodEntity> Breaks { get; set; } = new List<ProviderBreakPeriodEntity>();
}
