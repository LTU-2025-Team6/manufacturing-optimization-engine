using ManufacturingOptimization.Common.Contracts;
using ManufacturingOptimization.Common.Enums;

namespace ManufacturingOptimization.ProviderSimulator.Abstractions;

public interface IProviderSimulationContext
{
    public ProviderModel Provider { get; set; }
    public Dictionary<ProcessType, double> StandardDurations { get; }
}