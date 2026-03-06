using ManufacturingOptimization.Common.Abstractions;
using Microsoft.Extensions.Logging;

namespace ManufacturingOptimization.Common.Services;

/// <summary>
/// Thread-safe simulation clock that supports time acceleration.
/// Time = SimulatedBase + (RealElapsed * SpeedMultiplier)
/// </summary>
public sealed class SimulationClock : ISimulationClock
{
    private readonly ILogger<SimulationClock> _logger;
    private readonly object _lock = new();
    
    // Base timestamps for time calculation
    private DateTime _realBaseTime;
    private DateTime _simulatedBaseTime;
    private double _speedMultiplier;

    public SimulationClock(ILogger<SimulationClock> logger)
    {
        _logger = logger;
        var now = DateTime.UtcNow;
        _realBaseTime = now;
        _simulatedBaseTime = now;
        _speedMultiplier = 1.0;
    }

    public DateTime UtcNow
    {
        get
        {
            lock (_lock)
            {
                var realElapsed = DateTime.UtcNow - _realBaseTime;
                var simulatedElapsed = TimeSpan.FromTicks((long)(realElapsed.Ticks * _speedMultiplier));
                return _simulatedBaseTime + simulatedElapsed;
            }
        }
    }

    public double SpeedMultiplier
    {
        get
        {
            lock (_lock)
                return _speedMultiplier;
        }
    }

    public void SetTime(DateTime simulatedUtcNow, double speedMultiplier)
    {
        if (speedMultiplier <= 0)
            throw new ArgumentException("Speed multiplier must be positive", nameof(speedMultiplier));

        lock (_lock)
        {
            _realBaseTime = DateTime.UtcNow;
            // Normalise to UTC: if the caller somehow passes Kind=Local (e.g. from a poorly
            // deserialised message), convert it first; otherwise just stamp Kind=Utc so that
            // comparisons against DB-read datetimes (also Kind=Utc) are always consistent.
            _simulatedBaseTime = simulatedUtcNow.Kind == DateTimeKind.Local
                ? simulatedUtcNow.ToUniversalTime()
                : DateTime.SpecifyKind(simulatedUtcNow, DateTimeKind.Utc);
            _speedMultiplier = speedMultiplier;
        }
    }
}
