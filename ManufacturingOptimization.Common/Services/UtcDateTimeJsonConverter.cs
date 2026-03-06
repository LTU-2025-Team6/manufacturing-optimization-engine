using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManufacturingOptimization.Common.Services;

/// <summary>
/// Ensures every DateTime round-tripped through RabbitMQ messages has DateTimeKind.Utc.
///
/// Read strategy (priority order):
///   1. TryGetDateTimeOffset — handles any offset string correctly:
///        "13:44Z"       → UtcDateTime = 13:44Z  ✓
///        "15:44+02:00"  → UtcDateTime = 13:44Z  ✓  (NOT just a re-label)
///   2. Fallback GetDateTime + SpecifyKind — for bare strings emitted by older message
///      publishers that omit the Z suffix.
///
/// Write strategy:
///   Always serialize with Z suffix so every consumer receives an unambiguous UTC string.
///   Without this, Kind=Unspecified would be written without Z, and the receiver's
///   GetDateTime() would return Kind=Unspecified, making SimulationClock comparisons
///   against Kind=Utc segment times appear to differ by the host UTC offset.
/// </summary>
internal sealed class UtcDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TryGetDateTimeOffset(out var dto))
            return DateTime.SpecifyKind(dto.UtcDateTime, DateTimeKind.Utc);

        // Fallback for bare strings: treat as UTC (matches write behaviour).
        return DateTime.SpecifyKind(reader.GetDateTime(), DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Always serialize with Z suffix so the recipient can round-trip as Kind=Utc.
        writer.WriteStringValue(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }
}
