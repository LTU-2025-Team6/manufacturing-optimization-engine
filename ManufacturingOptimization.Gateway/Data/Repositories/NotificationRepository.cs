using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Services;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;
using ManufacturingOptimization.Gateway.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Gateway.Data.Repositories;

/// <summary>
/// Repository for Notification entities.
/// </summary>
public class NotificationRepository : Repository<NotificationEntity>, INotificationRepository
{
    public NotificationRepository(IDbContext context) : base(context)
    {
    }

    public async Task<List<NotificationEntity>> GetNewNotificationsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(n => !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<NotificationEntity>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<NotificationEntity>> GetSinceAsync(DateTime since, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(n => n.CreatedAt >= since)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<NotificationEntity> items, int totalCount)> GetPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        var totalCount = await _dbSet.CountAsync(cancellationToken);
        var items = await _dbSet
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _dbSet.FindAsync(new object[] { notificationId }, cancellationToken);
        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAllAsReadAsync(CancellationToken cancellationToken = default)
    {
        var unreadNotifications = await _dbSet
            .Where(n => !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public override async Task<IEnumerable<NotificationEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
