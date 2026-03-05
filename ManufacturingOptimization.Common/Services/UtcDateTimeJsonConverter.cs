using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManufacturingOptimization.Common.Services;

/// <summary>
/// Ensures every DateTime round-tripped through RabbitMQ messages has DateTimeKind.Utc.
///
/// Problem without this converter:
///   - JsonSerializer serializes Kind=Utc as "...Z" and Kind=Unspecified as "..." (no Z).
///   - On the receiving side, "..." (no Z) is deserialized as Kind=Unspecified.
///   - SimulationClock.UtcNow inherits that Kind, while DB-read segment times have Kind=Utc.
///   - C# DateTime comparison IGNORES Kind, so "07:27 Unspecified" vs "05:27 Utc" looks like
///     a 120-minute gap, causing false "execution overdue" failures.
///
/// Fix: always write with Z suffix (forces Kind=Utc), always read back as Kind=Utc.
/// </summary>
internal sealed class UtcDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // GetDateTime converts offset strings to UTC per .NET docs.
        // For bare strings (no offset/Z), returns Unspecified — SpecifyKind normalizes to Utc.
        return DateTime.SpecifyKind(reader.GetDateTime(), DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Always serialize with Z suffix so the recipient can round-trip as Kind=Utc.
        writer.WriteStringValue(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }
}
