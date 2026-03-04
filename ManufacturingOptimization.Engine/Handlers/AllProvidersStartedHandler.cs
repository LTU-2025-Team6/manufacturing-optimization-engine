using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;

namespace ManufacturingOptimization.Engine.Handlers;

public class AllProvidersStartedHandler : IMessageHandler<AllProvidersStartedEvent>
{
    private readonly ISystemReadinessService _systemReadinessService;

    public AllProvidersStartedHandler(
        ISystemReadinessService systemReadinessService)
    {
        _systemReadinessService = systemReadinessService;
    }

    public Task HandleAsync(AllProvidersStartedEvent evt)
    {
        _systemReadinessService.MarkProvidersReady();
        return Task.CompletedTask;
    }
}
