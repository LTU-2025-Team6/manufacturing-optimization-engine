using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Common.Models.Extensions;

public static class WorkingHoursExtensions
{
    public static List<ProviderScheduleSegmentModel> GetBreakSegments(this ProviderWorkingHoursModel workingHours, DateTime windowStart, DateTime windowEnd)
    {
        var result = new List<ProviderScheduleSegmentModel>();

        if (workingHours.Is24x7)
            return result;

        var currentDay = windowStart.Date;
        var lastDay = windowEnd.Date;

        while (currentDay <= lastDay)
        {
            var dayStart = currentDay;
            var dayEnd = currentDay.AddDays(1);

            if (!workingHours.WorkingDays.Contains(currentDay.DayOfWeek))
            {
                // All day is non-working
                result.Add(NewSegment(Max(windowStart, dayStart), Min(windowEnd, dayEnd), SegmentType.Break));

                currentDay = currentDay.AddDays(1);
                continue;
            }

            var workStart = currentDay.AddHours(workingHours.WorkDayStartHour);
            var workEnd = currentDay.AddHours(workingHours.WorkDayEndHour);

            // Time before the start of the working day
            if (windowStart < workStart)
            {
                result.Add(NewSegment(Max(windowStart, dayStart), Min(windowEnd, workStart), SegmentType.Break));
            }

            // Breaks within the working day
            var breaks = workingHours.GetBreaksForDay(currentDay, workStart, workEnd);
            foreach (var currentBreak in breaks)
            {
                result.Add(NewSegment(Max(currentBreak.Start, windowStart), Min(currentBreak.End, windowEnd), SegmentType.Break));
            }

            // Time after the end of the working day
            if (windowEnd > workEnd)
            {
                result.Add(NewSegment(Max(workEnd, windowStart), Min(windowEnd, dayEnd), SegmentType.Break));
            }

            currentDay = currentDay.AddDays(1);
        }

        // Merge all overlapping or adjacent Breaks
        return MergeAdjacentBreaks(result);
    }

    private static List<ProviderScheduleSegmentModel> MergeAdjacentBreaks(List<ProviderScheduleSegmentModel> breaks)
    {
        var ordered = breaks.OrderBy(b => b.StartTime).ToList();
        var merged = new List<ProviderScheduleSegmentModel>();

        foreach (var b in ordered)
        {
            if (merged.Count == 0)
            {
                merged.Add(b);
                continue;
            }

            var last = merged.Last();
            if (b.StartTime <= last.EndTime) // overlapping or adjacent
            {
                last.EndTime = Max(last.EndTime, b.EndTime);
            }
            else
            {
                merged.Add(b);
            }
        }

        return merged;
    }

    private static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;

    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;

    private static List<(DateTime Start, DateTime End)> GetBreaksForDay(this ProviderWorkingHoursModel workingHours, DateTime date, DateTime rangeStart, DateTime rangeEnd)
    {
        return workingHours.Breaks
            .Select(b => (
                Start: b.GetStartTime(date),
                End: b.GetEndTime(date)
            ))
            .Where(b =>
                b.Start < rangeEnd &&
                b.End > rangeStart)
            .OrderBy(b => b.Start)
            .ToList();
    }

    private static ProviderScheduleSegmentModel NewSegment(DateTime start, DateTime end, SegmentType type)
        => new()
        {
            StartTime = start,
            EndTime = end,
            SegmentType = type
        };
}
