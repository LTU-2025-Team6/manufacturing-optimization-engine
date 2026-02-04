using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;

namespace ManufacturingOptimization.Gateway.Handlers;

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
