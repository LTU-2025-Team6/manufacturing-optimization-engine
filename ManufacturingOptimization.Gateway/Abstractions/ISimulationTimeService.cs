using ManufacturingOptimization.Gateway.DTOs;

namespace ManufacturingOptimization.Gateway.Abstractions;

public interface ISimulationTimeService
{
    SimulationTimeDto GetCurrentTime();
    void SetTime(DateTime? simulatedUtcNow, double? speedMultiplier);
}
