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
            // Always normalize to UTC kind so UtcNow comparisons with DB-read datetimes
            // (which always have Kind=Utc via the ValueConverter) are consistent.
            // Without this, a bare "07:27" deserialized as Unspecified would produce
            // UtcNow with Kind=Unspecified, causing false "overdue" failures when
            // compared to segment StartTime values that have Kind=Utc.
            _simulatedBaseTime = DateTime.SpecifyKind(simulatedUtcNow, DateTimeKind.Utc);
            _speedMultiplier = speedMultiplier;
        }
    }
}
