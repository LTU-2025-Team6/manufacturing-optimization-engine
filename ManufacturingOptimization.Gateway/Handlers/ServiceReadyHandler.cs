using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;

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
