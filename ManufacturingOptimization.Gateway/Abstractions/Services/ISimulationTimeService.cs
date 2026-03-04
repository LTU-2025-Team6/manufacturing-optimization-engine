using ManufacturingOptimization.Gateway.DTOs.System;

namespace ManufacturingOptimization.Gateway.Abstractions.Services;

public interface ISimulationTimeService
{
    SimulationTimeDto GetCurrentTime();
    void SetTime(DateTime? simulatedUtcNow, double? speedMultiplier);
}
