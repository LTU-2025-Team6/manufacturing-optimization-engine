using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;

namespace ManufacturingOptimization.Gateway.Handlers;

/// <summary>
/// Handles service ready events and publishes notifications.
/// </summary>
public class ServiceReadyHandler : IMessageHandler<ServiceReadyEvent>
{
    private readonly ILogger<ServiceReadyHandler> _logger;

    public ServiceReadyHandler(
        ILogger<ServiceReadyHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(ServiceReadyEvent evt)
    {
        return Task.CompletedTask;
    }
}
