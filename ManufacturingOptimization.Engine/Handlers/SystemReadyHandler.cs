using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;

namespace ManufacturingOptimization.Engine.Handlers;

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
