using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Common.Messaging.Messages;

public static class NotificationRoutingKeys
{
    public const string CreateNotification = "notification.create";
}

public class CreateNotificationCommand : BaseCommand
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Info;
    public string Source { get; set; } = string.Empty;
}