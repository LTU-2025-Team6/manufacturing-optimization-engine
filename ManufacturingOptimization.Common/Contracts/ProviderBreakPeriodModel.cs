namespace ManufacturingOptimization.Common.Contracts;

public class ProviderBreakPeriodModel
{
    public int StartHour { get; set; }
    public int StartMinute { get; set; }
    public int DurationMinutes { get; set; }
    public string Name { get; set; } = "Break";
    
    public DateTime GetStartTime(DateTime date)
    {
        // Create DateTime with UTC kind - constructor returns Unspecified!
        return new DateTime(date.Year, date.Month, date.Day, StartHour, StartMinute, 0, DateTimeKind.Utc);
    }

    public DateTime GetEndTime(DateTime date)
    {
        return GetStartTime(date).AddMinutes(DurationMinutes);
    }
}
