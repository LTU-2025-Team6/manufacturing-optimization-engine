using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;

namespace ManufacturingOptimization.Gateway.Handlers;

public class SystemReadyHandler : IMessageHandler<SystemReadyEvent>
{
    private readonly ISystemReadinessService _systemReadinessService;

    public SystemReadyHandler(
        ISystemReadinessService systemReadinessService)
    {
        _systemReadinessService = systemReadinessService;
    }

    public Task HandleAsync(SystemReadyEvent evt)
    {
        _systemReadinessService.MarkSystemReady();
        return Task.CompletedTask;
    }
}