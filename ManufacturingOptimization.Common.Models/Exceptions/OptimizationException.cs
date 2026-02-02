namespace ManufacturingOptimization.Common.Models.Exceptions;

/// <summary>
/// Exception thrown when optimization cannot be completed.
/// </summary>
public class OptimizationException : Exception
{
    public OptimizationException(string message) : base(message)
    {
    }

    public OptimizationException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
