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
        var currentTime = _clock.UtcNow;
        var currentSpeed = _clock.SpeedMultiplier;

        var newTime = simulatedUtcNow ?? currentTime;
        var newSpeed = speedMultiplier ?? currentSpeed;

        if (newSpeed <= 0)
            throw new ArgumentException("Speed multiplier must be positive", nameof(speedMultiplier));

        // Update local clock
        _clock.SetTime(newTime, newSpeed);

        // Broadcast to all services (Engine, ProviderSimulators)
        _publisher.Publish(Exchanges.System, SystemRoutingKeys.TimeChanged, 
            new SimulationTimeChangedEvent
            {
                SimulatedUtcNow = newTime,
                SpeedMultiplier = newSpeed
            });

        _logger.LogInformation("🌐 Simulation time broadcast: {Time:O}, ×{Speed}", newTime, newSpeed);
    }
}
