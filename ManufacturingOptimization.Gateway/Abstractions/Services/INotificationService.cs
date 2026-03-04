using ManufacturingOptimization.Gateway.DTOs.Common;
using ManufacturingOptimization.Gateway.DTOs.Notification;

namespace ManufacturingOptimization.Gateway.Abstractions.Services;

public interface INotificationService
{
    Task<PagedResult<NotificationPreviewDto>> GetAllNotificationsAsync(PaginationRequest pagination);
    Task<IEnumerable<NotificationPreviewDto>> GetNewNotificationsAsync();
    Task<IEnumerable<NotificationPreviewDto>> GetRecentNotificationsAsync();
    Task<IEnumerable<NotificationPreviewDto>> GetNotificationsSinceAsync(DateTime since);
    Task<NotificationDto> GetNotificationByIdAsync(Guid id);
    Task MarkAsReadAsync(Guid id);
    Task MarkAllAsReadAsync();
}
