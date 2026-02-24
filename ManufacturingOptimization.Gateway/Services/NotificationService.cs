using AutoMapper;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.DTOs;
using ManufacturingOptimization.Gateway.Exceptions;

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

    public async Task<List<NotificationDto>> GetAllNotificationsAsync()
    {
        var notifications = await _notificationRepository.GetAllAsync();
        return _mapper.Map<List<NotificationDto>>(notifications);
    }

    public async Task<List<NotificationDto>> GetNewNotificationsAsync()
    {
        var notifications = await _notificationRepository.GetNewNotificationsAsync();
        return _mapper.Map<List<NotificationDto>>(notifications);
    }

    public async Task<List<NotificationDto>> GetRecentNotificationsAsync(int count = 10)
    {
        var notifications = await _notificationRepository.GetRecentAsync(count);
        return _mapper.Map<List<NotificationDto>>(notifications);
    }

    public async Task<List<NotificationDto>> GetTwoWeeksNotificationsAsync()
    {
        var notifications = await _notificationRepository.GetTwoWeeksAsync();
        return _mapper.Map<List<NotificationDto>>(notifications);
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
