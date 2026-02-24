using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.ExecutionManagement;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Gateway.Data;
using ManufacturingOptimization.Gateway.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Gateway.Handlers;

public class ExecutionEventHandler : 
    IMessageHandler<ExecutionStepStartedEvent>,
    IMessageHandler<ExecutionStepCompletedEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<ExecutionHub> _hubContext;
    private readonly ILogger<ExecutionEventHandler> _logger;

    public ExecutionEventHandler(IServiceScopeFactory scopeFactory, IHubContext<ExecutionHub> hubContext, ILogger<ExecutionEventHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task HandleAsync(ExecutionStepStartedEvent @event)
    {
        _logger.LogInformation($"📡 [Gateway] Step {@event.StepNumber} Started. Broadcasting to UI...");
        
        await UpdateStepStatus(@event.StepId, StepExecutionStatus.InProgress);
        await _hubContext.Clients.All.SendAsync("StepStarted", @event);
    }

    public async Task HandleAsync(ExecutionStepCompletedEvent @event)
    {
        _logger.LogInformation($"📡 [Gateway] Step {@event.StepNumber} Completed. Broadcasting to UI...");
        
        var status = @event.Success ? StepExecutionStatus.Completed : StepExecutionStatus.Failed;
        await UpdateStepStatus(@event.StepId, status);
        await _hubContext.Clients.All.SendAsync("StepCompleted", @event);
    }

    private async Task UpdateStepStatus(Guid stepId, StepExecutionStatus status)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GatewayDbContext>();
        
        var step = await db.ProcessSteps.FirstOrDefaultAsync(s => s.Id == stepId);
        if (step != null)
        {
            step.ExecutionStatus = status;
            await db.SaveChangesAsync();
        }
    }
}