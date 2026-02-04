using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.ProviderSimulator.Abstractions;

public interface IProviderSimulationContext
{
    public ProviderModel Provider { get; set; }
    public Dictionary<ProcessType, double> StandardDurations { get; }
}