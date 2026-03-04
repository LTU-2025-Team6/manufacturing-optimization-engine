using ManufacturingOptimization.Common.Enums;

namespace ManufacturingOptimization.Gateway.Data.Entities;

/// <summary>
/// Notification entity for database storage.
/// Stores system notifications that can be displayed to users.
/// </summary>
public class NotificationEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Info;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Source/Author of the notification (e.g., "Provider", "Engine", "Gateway")
    /// </summary>
    public string Source { get; set; } = string.Empty;
}
