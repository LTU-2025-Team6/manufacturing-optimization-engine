using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using Microsoft.Extensions.Logging;

namespace ManufacturingOptimization.ProviderSimulator.Handlers;

/// <summary>
/// DEPRECATED: Execution is now handled by ExecutionSchedulerService.
/// This handler is kept for backward compatibility but does nothing.
/// </summary>
public class ProcessExecutionHandler : IMessageHandler<ExecuteProcessCommand>
{
    private readonly ILogger<ProcessExecutionHandler> _logger;

    public ProcessExecutionHandler(ILogger<ProcessExecutionHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(ExecuteProcessCommand message)
    {
        _logger.LogWarning(
            "⚠️ Received ExecuteProcessCommand for {Process} - This handler is deprecated. " +
            "Execution is now managed by ExecutionSchedulerService based on schedule.",
            message.ProcessName);

        return Task.CompletedTask;
    }
}