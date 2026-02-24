using ManufacturingOptimization.Gateway.DTOs;

namespace ManufacturingOptimization.Gateway.Abstractions;

public interface INotificationService
{
    Task<List<NotificationDto>> GetAllNotificationsAsync();
    Task<List<NotificationDto>> GetNewNotificationsAsync();
    Task<List<NotificationDto>> GetRecentNotificationsAsync(int count = 10);
    Task<List<NotificationDto>> GetTwoWeeksNotificationsAsync();
    Task<NotificationDto> GetNotificationByIdAsync(Guid id);
    Task MarkAsReadAsync(Guid id);
    Task MarkAllAsReadAsync();
}
