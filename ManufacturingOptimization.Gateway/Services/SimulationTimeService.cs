using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Gateway.Abstractions.Services;
using ManufacturingOptimization.Gateway.DTOs.System;

namespace ManufacturingOptimization.Gateway.Services;

public class SimulationTimeService : ISimulationTimeService
{
    private readonly ISimulationClock _clock;
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<SimulationTimeService> _logger;

    public SimulationTimeService(
        ISimulationClock clock,
        IMessagePublisher publisher,
        ILogger<SimulationTimeService> logger)
    {
        _clock = clock;
        _publisher = publisher;
        _logger = logger;
    }

    public SimulationTimeDto GetCurrentTime()
    {
        return new SimulationTimeDto
        {
            SimulatedUtcNow = _clock.UtcNow,
            SpeedMultiplier = _clock.SpeedMultiplier
        };
    }

    public void SetTime(DateTime? simulatedUtcNow, double? speedMultiplier)
    {
        var newTime  = simulatedUtcNow ?? _clock.UtcNow;
        var newSpeed = speedMultiplier ?? _clock.SpeedMultiplier;

        if (newSpeed <= 0)
            throw new ArgumentException("Speed multiplier must be positive", nameof(speedMultiplier));

        _clock.SetTime(newTime, newSpeed);

        _publisher.Publish(Exchanges.System, SystemRoutingKeys.TimeChanged,
            new SimulationTimeChangedEvent
            {
                SimulatedUtcNow = newTime,
                SpeedMultiplier = newSpeed
            });
    }
}
