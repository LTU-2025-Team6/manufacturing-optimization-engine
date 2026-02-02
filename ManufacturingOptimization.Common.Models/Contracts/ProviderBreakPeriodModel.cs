namespace ManufacturingOptimization.Common.Models.Contracts;

public class ProviderBreakPeriodModel
{
    public int StartHour { get; set; }
    public int StartMinute { get; set; }
    public int DurationMinutes { get; set; }
    public string Name { get; set; } = "Break";
    
    public DateTime GetStartTime(DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, StartHour, StartMinute, 0);
    }

    public DateTime GetEndTime(DateTime date)
    {
        return GetStartTime(date).AddMinutes(DurationMinutes);
    }
}
