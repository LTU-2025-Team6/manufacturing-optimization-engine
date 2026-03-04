using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Gateway.Data.Entities;

namespace ManufacturingOptimization.Gateway.Abstractions.Repositories;

public interface INotificationRepository : IRepository<NotificationEntity>
{
    Task<List<NotificationEntity>> GetNewNotificationsAsync(CancellationToken cancellationToken = default);
    Task<List<NotificationEntity>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
    Task<List<NotificationEntity>> GetSinceAsync(DateTime since, CancellationToken cancellationToken = default);
    Task<(List<NotificationEntity> items, int totalCount)> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(CancellationToken cancellationToken = default);
}
