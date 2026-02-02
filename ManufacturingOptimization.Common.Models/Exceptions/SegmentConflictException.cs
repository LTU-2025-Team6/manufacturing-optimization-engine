namespace ManufacturingOptimization.Common.Models.Exceptions;

/// <summary>
/// Exception thrown when there is a conflict in timeline segment operations.
/// </summary>
public class SegmentConflictException : Exception
{
    public SegmentConflictException(string message) : base(message)
    {
    }

    public SegmentConflictException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
