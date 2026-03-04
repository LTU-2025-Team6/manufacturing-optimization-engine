using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Enums;

namespace ManufacturingOptimization.Common.Messages;

public static class NotificationRoutingKeys
{
    public const string CreateNotification = "notification.create";
}

public class CreateNotificationCommand : IMessage
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Info;
    public string Source { get; set; } = string.Empty;
}