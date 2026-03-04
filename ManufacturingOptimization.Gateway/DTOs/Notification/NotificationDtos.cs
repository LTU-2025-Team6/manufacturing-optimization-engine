using ManufacturingOptimization.Common.Enums;

namespace ManufacturingOptimization.Gateway.DTOs.Notification;

/// <summary>
/// Preview information about a notification for list views.
/// </summary>
public class NotificationPreviewDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Source { get; set; } = string.Empty;
}

/// <summary>
/// Complete notification with full message.
/// </summary>
public class NotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public string Source { get; set; } = string.Empty;
}
