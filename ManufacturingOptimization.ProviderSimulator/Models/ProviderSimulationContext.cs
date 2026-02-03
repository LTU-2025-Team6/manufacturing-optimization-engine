using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Settings;
using Microsoft.Extensions.Options;

namespace ManufacturingOptimization.ProviderSimulator.Models;

public class ProviderSimulationContext : IProviderSimulationContext
{
    protected readonly Random _random = new();

    public ProviderModel Provider { get; }
    public Dictionary<ProcessType, double> StandardDurations { get; }

    public ProviderSimulationContext(
        IOptions<ProviderSettings> providerSettings,
        IOptions<ProviderTypeSettings> providerTypeSettings,
        IOptions<ProcessStandardsSettings> processStandards)
    {
        StandardDurations = processStandards.Value.StandardDurationHours;

        Provider = new ProviderModel
        {
            Id = Guid.Parse(providerSettings.Value.ProviderId),
            Type = providerTypeSettings.Value.Type,
            Name = providerSettings.Value.ProviderName,
            ProcessCapabilities = providerSettings.Value.ProcessCapabilities,
            TechnicalCapabilities = providerSettings.Value.TechnicalCapabilities,
            WorkingHours = providerSettings.Value.WorkingHours
        };
    }
}
