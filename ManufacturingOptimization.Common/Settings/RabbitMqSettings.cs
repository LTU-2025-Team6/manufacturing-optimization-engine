namespace ManufacturingOptimization.Common.Settings;

/// <summary>
/// RabbitMQ connection settings.
/// Configure via environment variables: RabbitMQ__Host, RabbitMQ__Port, RabbitMQ__Username, RabbitMQ__Password
/// </summary>
public class RabbitMqSettings
{
    public const string SectionName = "RabbitMQ";
    
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}