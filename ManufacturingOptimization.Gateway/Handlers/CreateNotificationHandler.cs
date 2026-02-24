using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;

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
