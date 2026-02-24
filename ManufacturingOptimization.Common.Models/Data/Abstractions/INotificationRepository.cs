using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.Common.Models.Data.Abstractions;

public interface INotificationRepository : IRepository<NotificationEntity>
{
    Task<List<NotificationEntity>> GetNewNotificationsAsync(CancellationToken cancellationToken = default);
    Task<List<NotificationEntity>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
    Task<List<NotificationEntity>> GetSinceAsync(DateTime since, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(CancellationToken cancellationToken = default);
}
