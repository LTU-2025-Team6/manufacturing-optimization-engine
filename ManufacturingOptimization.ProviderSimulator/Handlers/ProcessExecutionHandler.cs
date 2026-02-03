using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using Microsoft.Extensions.Logging;

namespace ManufacturingOptimization.ProviderSimulator.Handlers;

public class ProcessExecutionHandler : IMessageHandler<ExecuteProcessCommand>
{
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<ProcessExecutionHandler> _logger;

    public ProcessExecutionHandler(IMessagePublisher publisher, ILogger<ProcessExecutionHandler> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task HandleAsync(ExecuteProcessCommand message)
    {
        _logger.LogInformation("⚙️ EXECUTION REQUEST: Provider received request to execute '{ProcessName}' (Step {StepId})", message.ProcessName, message.StepId);

        // Simulate Work Duration
        var simulationDelay = TimeSpan.FromSeconds(5); 
        _logger.LogInformation("... Work in progress for {Delay}s ...", simulationDelay.TotalSeconds);
        
        await Task.Delay(simulationDelay);

        // Create Completion Event
        var reply = new ProcessExecutionCompletedEvent
        {
            PlanId = message.PlanId,
            StepId = message.StepId,
            ProviderId = message.TargetProviderId,
            Success = true,
            CompletedAt = DateTime.UtcNow
        };

        // Send Reply directly to the Engine
        _logger.LogInformation("✅ WORK COMPLETE: Sending completion signal for '{ProcessName}'", message.ProcessName);
        _publisher.PublishReply(message, reply);
    }
}