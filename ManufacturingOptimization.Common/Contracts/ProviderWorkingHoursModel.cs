namespace ManufacturingOptimization.Common.Contracts;

public class ProviderWorkingHoursModel
{
    public HashSet<DayOfWeek> WorkingDays { get; set; } = new()
    {
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday
    };

    public int WorkDayStartHour { get; set; }
    public int WorkDayEndHour { get; set; }
    public bool Is24x7 { get; set; } = false;
    public List<ProviderBreakPeriodModel> Breaks { get; set; } = [];
}