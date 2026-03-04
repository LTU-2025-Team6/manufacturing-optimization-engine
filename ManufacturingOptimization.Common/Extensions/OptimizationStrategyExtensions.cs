using ManufacturingOptimization.Common.Contracts;
using ManufacturingOptimization.Common.Enums;

namespace ManufacturingOptimization.Common.Extensions;

public static class OptimizationStrategyExtensions
{
    public static void ClampSchedulesToWorkingTimeline(this OptimizationStrategyModel strategy)
    {
        if (strategy?.Steps == null || strategy.Steps.Count == 0)
            return;

        // Find all scheduled steps
        var scheduledSteps = strategy.Steps
            .Where(s => s.AllocatedSchedule != null)
            .ToList();

        if (scheduledSteps.Count == 0)
            return;

        // Find the earliest WorkingTime start across all steps
        var earliestWorkStart = scheduledSteps
            .SelectMany(s => s.AllocatedSchedule!.Segments
                .Where(seg => seg.SegmentType == SegmentType.WorkingTime))
            .Select(seg => seg.StartTime)
            .DefaultIfEmpty(DateTime.MaxValue)
            .Min();

        // Find the latest WorkingTime end across all steps
        var latestWorkEnd = scheduledSteps
            .SelectMany(s => s.AllocatedSchedule!.Segments
                .Where(seg => seg.SegmentType == SegmentType.WorkingTime))
            .Select(seg => seg.EndTime)
            .DefaultIfEmpty(DateTime.MinValue)
            .Max();

        // If no WorkingTime segments found, return
        if (earliestWorkStart == DateTime.MaxValue || latestWorkEnd == DateTime.MinValue)
            return;

        // Clamp each step's schedule to the working time range
        foreach (var step in scheduledSteps)
        {
            var schedule = step.AllocatedSchedule!;
            
            // Clamp segments using the extension method
            var clampedSegments = schedule.Segments.ClampToTimeRange(earliestWorkStart, latestWorkEnd);

            // Update the schedule with clamped values
            step.AllocatedSchedule = new ProviderScheduleModel
            {
                Id = schedule.Id,
                Segments = clampedSegments
            };
        }
    }
}
