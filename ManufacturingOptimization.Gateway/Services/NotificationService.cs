using AutoMapper;
using ManufacturingOptimization.Gateway.DTOs.Common;
using ManufacturingOptimization.Gateway.DTOs.Notification;
using ManufacturingOptimization.Gateway.Exceptions;
using ManufacturingOptimization.Gateway.Abstractions.Services;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;

namespace ManufacturingOptimization.Gateway.Services;

public class NotificationService : INotificationService
{
    private readonly IMapper _mapper;
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(
        IMapper mapper,
        INotificationRepository notificationRepository)
    {
        _mapper = mapper;
        _notificationRepository = notificationRepository;
    }

    public async Task<PagedResult<NotificationPreviewDto>> GetAllNotificationsAsync(PaginationRequest pagination)
    {
        var skip = (pagination.PageNumber - 1) * pagination.PageSize;
        var (notifications, totalCount) = await _notificationRepository.GetPagedAsync(skip, pagination.PageSize);
        return new PagedResult<NotificationPreviewDto>(
            _mapper.Map<List<NotificationPreviewDto>>(notifications),
            pagination.PageNumber, pagination.PageSize, totalCount);
    }

    public async Task<IEnumerable<NotificationPreviewDto>> GetNewNotificationsAsync()
    {
        var notifications = await _notificationRepository.GetNewNotificationsAsync();
        return _mapper.Map<List<NotificationPreviewDto>>(notifications);
    }

    public async Task<IEnumerable<NotificationPreviewDto>> GetRecentNotificationsAsync()
    {
        var notifications = await _notificationRepository.GetAllAsync();
        return _mapper.Map<List<NotificationPreviewDto>>(notifications);
    }

    public async Task<IEnumerable<NotificationPreviewDto>> GetNotificationsSinceAsync(DateTime since)
    {
        var notifications = await _notificationRepository.GetSinceAsync(since);
        return _mapper.Map<List<NotificationPreviewDto>>(notifications);
    }

    public async Task<NotificationDto> GetNotificationByIdAsync(Guid id)
    {
        var notification = await _notificationRepository.GetByIdAsync(id);

        if (notification == null)
            throw new NotFoundException($"Notification with Id {id} not found.");

        return _mapper.Map<NotificationDto>(notification);
    }

    public async Task MarkAsReadAsync(Guid id)
    {
        await _notificationRepository.MarkAsReadAsync(id);
    }

    public async Task MarkAllAsReadAsync()
    {
        await _notificationRepository.MarkAllAsReadAsync();
    }
}
