using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManufacturingOptimization.Gateway.Extensions;

/// <summary>
/// Ensures every DateTime round-tripped through the HTTP API has DateTimeKind.Utc.
///
/// Read strategy (priority order):
///   1. TryGetDateTimeOffset — handles any offset string correctly:
///        "13:44Z"       → UtcDateTime = 13:44Z  ✓
///        "15:44+02:00"  → UtcDateTime = 13:44Z  ✓  (offset applied, value NOT shifted)
///   2. Fallback GetDateTime + SpecifyKind — for bare strings (no offset / no Z):
///        "13:44"        → 13:44Z (treated as UTC — frontend is assumed to always send UTC)
///
/// Write strategy:
///   Always serialize with Z suffix so any client receives an unambiguous UTC string.
/// </summary>
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Preferred path: offset-aware parse gives exact UTC regardless of the server's
        // local timezone.  GetDateTime() alone for "+02:00" strings returns Kind=Local whose
        // *value* is already in host-local time, so SpecifyKind would stamp the wrong hour.
        if (reader.TryGetDateTimeOffset(out var dto))
            return DateTime.SpecifyKind(dto.UtcDateTime, DateTimeKind.Utc);

        // Fallback for bare strings ("2026-04-26T13:44:51") — frontend always sends UTC,
        // so treating Unspecified as UTC is correct and consistent.
        return DateTime.SpecifyKind(reader.GetDateTime(), DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }
}
