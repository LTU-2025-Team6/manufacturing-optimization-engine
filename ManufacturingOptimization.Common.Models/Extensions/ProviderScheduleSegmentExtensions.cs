using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Common.Models.Exceptions;

namespace ManufacturingOptimization.Common.Models.Extensions;

public static class ProviderScheduleSegmentExtensions
{
    public enum SubtractResultMode
    {
        ToFreeSpace,
        Granular
    }

    public enum OverlayMode
    {
        Strict,
        ForceOverlay
    }

    public static List<ProviderScheduleSegmentModel> Subtract(this IReadOnlyCollection<ProviderScheduleSegmentModel> source, ICollection<ProviderScheduleSegmentModel> toSubtract, SubtractResultMode mode)
    {
        var result = source
            .OrderBy(s => s.StartTime)
            .ToList();

        ValidateTimeline(result);

        foreach (var subtract in toSubtract)
        {
            ValidateOverlayBounds(result, subtract);

            var affected = result
                .Where(s => Intersects(s, subtract))
                .ToList();

            foreach (var s in affected)
            {
                if (s.SegmentType != subtract.SegmentType)
                {
                    throw new SegmentConflictException(
                        $"Subtract conflict: {subtract.SegmentType} overlaps {s.SegmentType}");
                }
            }

            var next = new List<ProviderScheduleSegmentModel>();

            foreach (var s in result)
            {
                if (!Intersects(s, subtract))
                {
                    next.Add(s);
                    continue;
                }

                // left fragment
                if (subtract.StartTime > s.StartTime)
                {
                    next.Add(NewSegment(
                        s.StartTime,
                        subtract.StartTime,
                        s.SegmentType));
                }

                // right fragment
                if (subtract.EndTime < s.EndTime)
                {
                    next.Add(NewSegment(
                        subtract.EndTime,
                        s.EndTime,
                        s.SegmentType));
                }

                if (mode == SubtractResultMode.ToFreeSpace)
                {
                    // gap between fragments
                    next.Add(NewSegment(
                        Max(s.StartTime, subtract.StartTime),
                        Min(s.EndTime, subtract.EndTime),
                        SegmentType.FreeSpace));
                }
            }

            result = MergeAdjacent(next);
        }

        return result;
    }

    public static List<ProviderScheduleSegmentModel> Merge(this IReadOnlyCollection<ProviderScheduleSegmentModel> source, ICollection<ProviderScheduleSegmentModel> toMerge)
    {
        var result = source
            .OrderBy(s => s.StartTime)
            .ToList();

        ValidateTimeline(result);

        foreach (var merge in toMerge)
        {
            ValidateOverlayBounds(result, merge);

            var affected = result
                .Where(s => Intersects(s, merge))
                .ToList();

            foreach (var s in affected)
            {
                if (s.SegmentType != merge.SegmentType)
                {
                    throw new SegmentConflictException($"Merge conflict: {merge.SegmentType} overlaps {s.SegmentType}");
                }
            }

            result = result
                .Where(s => !Intersects(s, merge))
                .ToList();

            result.Add(NewSegment(
                merge.StartTime,
                merge.EndTime,
                merge.SegmentType));

            result = MergeAdjacent(result);
        }

        return result;
    }

    public static List<ProviderScheduleSegmentModel> Overlay(this IReadOnlyCollection<ProviderScheduleSegmentModel> source, ICollection<ProviderScheduleSegmentModel> overlay, OverlayMode mode = OverlayMode.Strict)
    {
        if (source == null || source.Count == 0)
            throw new SegmentConflictException("Source timeline is empty");

        var result = source
            .OrderBy(s => s.StartTime)
            .ToList();

        ValidateTimeline(result);

        foreach (var segment in overlay)
        {
            result = ApplyOverlay(result, segment, mode);
        }

        return result;
    }

    private static List<ProviderScheduleSegmentModel> ApplyOverlay(List<ProviderScheduleSegmentModel> source, ProviderScheduleSegmentModel overlay, OverlayMode mode)
    {
        ValidateOverlayBounds(source, overlay);

        var affected = source
            .Where(s => Intersects(s, overlay))
            .OrderBy(s => s.StartTime)
            .ToList();

        if (mode == OverlayMode.Strict)
        {
            foreach (var s in affected)
            {
                if (s.SegmentType != SegmentType.FreeSpace &&
                    s.SegmentType != overlay.SegmentType)
                {
                    throw new SegmentConflictException($"Conflict: {overlay.SegmentType} overlaps {s.SegmentType} [{overlay.StartTime} – {overlay.EndTime}]");
                }
            }
        }

        var result = new List<ProviderScheduleSegmentModel>();

        foreach (var s in source)
        {
            if (!Intersects(s, overlay))
            {
                result.Add(s);
                continue;
            }

            // same type keep as-is
            if (s.SegmentType == overlay.SegmentType)
            {
                result.Add(s);
                continue;
            }

            // In ForceOverlay mode, completely replace overlapping segments with overlay type
            // In Strict mode, only split FreeSpace
            if (mode == OverlayMode.ForceOverlay || s.SegmentType == SegmentType.FreeSpace)
            {
                // left fragment (keep original type)
                if (overlay.StartTime > s.StartTime)
                {
                    result.Add(NewSegment(
                        s.StartTime,
                        overlay.StartTime,
                        s.SegmentType,
                        s.ExecutionId));
                }

                // middle part (overlay type) - preserve ExecutionId from overlay
                result.Add(NewSegment(
                    Max(s.StartTime, overlay.StartTime),
                    Min(s.EndTime, overlay.EndTime),
                    overlay.SegmentType,
                    overlay.ExecutionId));

                // right fragment (keep original type)
                if (overlay.EndTime < s.EndTime)
                {
                    result.Add(NewSegment(
                        overlay.EndTime,
                        s.EndTime,
                        s.SegmentType,
                        s.ExecutionId));
                }
            }
        }

        return MergeAdjacent(result);
    }


    public static IReadOnlyList<IReadOnlyList<ProviderScheduleSegmentModel>> BuildPossibleWorkSlots(this IReadOnlyList<ProviderScheduleSegmentModel> timeline, double requiredWorkingHours, int stepMinutes)
    {
        var result = new List<IReadOnlyList<ProviderScheduleSegmentModel>>();

        if (timeline == null || timeline.Count == 0)
            return result;

        var orderedTimeline = timeline
            .OrderBy(s => s.StartTime)
            .ToList();

        var startTime = orderedTimeline.First().StartTime;
        var endTime = orderedTimeline.Last().EndTime;

        var current = startTime;

        while (current + TimeSpan.FromHours(requiredWorkingHours) <= endTime)
        {
            var slotTimeline = orderedTimeline.TryBuildWorkSlot(current, requiredWorkingHours);

            if (slotTimeline != null)
            {
                var isDuplicate = result.Any(existing => AreTimelinesEqual(existing, slotTimeline));

                if (!isDuplicate)
                {
                    result.Add(slotTimeline);
                }
            }

            current = current.AddMinutes(stepMinutes);
        }

        return result;
    }

    public static IReadOnlyList<ProviderScheduleSegmentModel>? TryBuildWorkSlot(this IReadOnlyList<ProviderScheduleSegmentModel> timeline, DateTime startTime, double requiredWorkingHours)
    {
        if (timeline == null || timeline.Count == 0)
            return null;

        var required = TimeSpan.FromHours(requiredWorkingHours);
        var accumulated = TimeSpan.Zero;
        var currentTime = startTime;

        // Copy timeline to avoid mutating the original
        var resultTimeline = timeline
            .OrderBy(s => s.StartTime)
            .Select(s => new ProviderScheduleSegmentModel
            {
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                SegmentType = s.SegmentType
            })
            .ToList();

        var newSegments = new List<ProviderScheduleSegmentModel>();

        foreach (var segment in resultTimeline)
        {
            // Skip segments before currentTime
            if (segment.EndTime <= currentTime)
                continue;

            // If we are before FreeSpace move forward to the start of FreeSpace
            if (segment.SegmentType != SegmentType.FreeSpace && currentTime < segment.EndTime)
            {
                currentTime = segment.EndTime;
                continue;
            }

            // Start accumulating
            if (segment.SegmentType == SegmentType.FreeSpace)
            {
                if (currentTime < segment.StartTime)
                    currentTime = segment.StartTime;

                var available = segment.EndTime - currentTime;
                var remaining = required - accumulated;

                if (available >= remaining)
                {
                    // Can complete the slot
                    newSegments.Add(NewSegment(currentTime, currentTime + remaining, SegmentType.WorkingTime));
                    accumulated += remaining;
                    currentTime += remaining;

                    try
                    {
                        resultTimeline = resultTimeline.Overlay(newSegments);
                    }
                    catch (SegmentConflictException)
                    {
                        return null;
                    }

                    return resultTimeline.AsReadOnly();
                }
                else
                {
                    // Use the entire segment
                    newSegments.Add(NewSegment(currentTime, segment.EndTime, SegmentType.WorkingTime));
                    accumulated += available;
                    currentTime = segment.EndTime;
                }
            }
            else
            {
                // just skip
                currentTime = segment.EndTime;
            }
        }

        // If we haven't accumulated enough working hours
        return null;
    }

    /// <summary>
    /// Clamps segments to a specified time range.
    /// Segments outside the range are removed, segments partially overlapping are trimmed.
    /// </summary>
    public static List<ProviderScheduleSegmentModel> ClampToTimeRange(this IReadOnlyList<ProviderScheduleSegmentModel> segments, DateTime startTime, DateTime endTime)
    {
        if (segments == null || segments.Count == 0)
            return new List<ProviderScheduleSegmentModel>();

        return segments
            .Where(s => s.EndTime > startTime && s.StartTime < endTime)
            .Select(s => new ProviderScheduleSegmentModel
            {
                StartTime = s.StartTime < startTime ? startTime : s.StartTime,
                EndTime = s.EndTime > endTime ? endTime : s.EndTime,
                SegmentType = s.SegmentType
            })
            .ToList();
    }

    private static void ValidateOverlayBounds(List<ProviderScheduleSegmentModel> timeline, ProviderScheduleSegmentModel overlay)
    {
        var start = timeline.First().StartTime;
        var end = timeline.Last().EndTime;

        if (overlay.StartTime < start && overlay.EndTime < end)
            overlay.StartTime = start;

        if (overlay.EndTime > end && overlay.StartTime > start)
            overlay.EndTime = end;

        if (overlay.StartTime < start || overlay.EndTime > end)
            throw new SegmentConflictException($"Overlay [{overlay.StartTime} – {overlay.EndTime}] outside timeline [{start} – {end}]");
    }

    private static void ValidateTimeline(List<ProviderScheduleSegmentModel> segments)
    {
        for (int i = 1; i < segments.Count; i++)
        {
            // Allow segments to touch (EndTime == StartTime) but not overlap
            if (segments[i - 1].EndTime > segments[i].StartTime)
                throw new SegmentConflictException("Timeline has overlaps");
        }
    }

    // Segments intersect only if they actually overlap (not just touch at boundaries)
    private static bool Intersects(ProviderScheduleSegmentModel a, ProviderScheduleSegmentModel b)
    {
        // Allow touching at boundaries (EndTime == StartTime) but not overlapping
        if (a.EndTime == b.StartTime || b.EndTime == a.StartTime)
            return false;
        
        return a.StartTime < b.EndTime && b.StartTime < a.EndTime;
    }

    private static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;

    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;

    private static ProviderScheduleSegmentModel NewSegment(DateTime start, DateTime end, SegmentType type, Guid? executionId = null)
        => new()
        {
            StartTime = start,
            EndTime = end,
            SegmentType = type,
            ExecutionId = executionId
        };

    private static List<ProviderScheduleSegmentModel> MergeAdjacent(List<ProviderScheduleSegmentModel> segments)
    {
        return segments
            .OrderBy(s => s.StartTime)
            .Aggregate(new List<ProviderScheduleSegmentModel>(), (acc, s) =>
            {
                if (!acc.Any())
                {
                    acc.Add(s);
                    return acc;
                }

                var last = acc.Last();

                if (last.SegmentType == s.SegmentType &&
                    last.EndTime == s.StartTime)
                {
                    last.EndTime = s.EndTime;
                }
                else
                {
                    acc.Add(s);
                }

                return acc;
            });
    }

    private static bool AreTimelinesEqual(IReadOnlyList<ProviderScheduleSegmentModel> a, IReadOnlyList<ProviderScheduleSegmentModel> b)
    {
        if (a.Count != b.Count)
            return false;

        for (int i = 0; i < a.Count; i++)
        {
            if (a[i].StartTime != b[i].StartTime ||
                a[i].EndTime != b[i].EndTime ||
                a[i].SegmentType != b[i].SegmentType)
                return false;
        }

        return true;
    }
}
