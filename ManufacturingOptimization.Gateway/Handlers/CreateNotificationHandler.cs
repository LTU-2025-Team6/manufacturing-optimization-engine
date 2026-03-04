using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;
using ManufacturingOptimization.Gateway.Data.Entities;

namespace ManufacturingOptimization.Gateway.Handlers;

public class CreateNotificationHandler : IMessageHandler<CreateNotificationCommand>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<CreateNotificationHandler> _logger;

    public CreateNotificationHandler(
        INotificationRepository notificationRepository,
        ILogger<CreateNotificationHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task HandleAsync(CreateNotificationCommand command)
    {
        _logger.LogInformation("Creating notification: {Title}", command.Title);

        var notification = new NotificationEntity
        {
            Id = Guid.NewGuid(),
            Title = command.Title,
            Message = command.Message,
            Type = command.Type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            Source = command.Source
        };

        await _notificationRepository.AddAsync(notification);
        await _notificationRepository.SaveChangesAsync();
    }
}
