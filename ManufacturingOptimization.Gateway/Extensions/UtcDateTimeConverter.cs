using System.Text.Json;
using System.Text.Json.Serialization;

namespace ManufacturingOptimization.Gateway.Extensions;

/// <summary>
/// Ensures every DateTime read from JSON is treated as UTC (DateTimeKind.Utc),
/// and written back as ISO 8601 with the Z suffix.
/// Prevents the 1-hour shift that occurs when the server runs in a non-UTC timezone.
/// </summary>
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetDateTime();
        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }
}
