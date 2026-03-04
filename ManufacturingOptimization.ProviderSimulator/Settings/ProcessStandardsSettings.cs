using ManufacturingOptimization.Common.Enums;
using System.Text.Json.Serialization;

namespace ManufacturingOptimization.ProviderSimulator.Settings;

/// <summary>
/// Standard values for manufacturing processes.
/// </summary>
public class ProcessStandardsSettings
{
    public const string SectionName = "ProcessStandards";

    /// <summary>
    /// Standard duration for each process type in hours.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Dictionary<ProcessType, double> StandardDurationHours { get; set; } = new();
}
