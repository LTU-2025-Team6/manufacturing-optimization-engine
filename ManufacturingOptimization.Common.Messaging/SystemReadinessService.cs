using ManufacturingOptimization.Common.Messaging.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManufacturingOptimization.Common.Messaging;

/// <summary>
/// Base class for system readiness coordination.
/// - Implements ISystemReadinessService with TaskCompletionSource for blocking operations
/// - Listens for SystemReadyEvent and marks service ready
/// - Listens for AllProvidersRegisteredEvent and marks providers ready
/// - Can be extended for additional startup logic (see StartupCoordinator in Engine)
/// </summary>
public class SystemReadinessService : BackgroundService, ISystemReadinessService
{
    private readonly TaskCompletionSource<bool> _systemReadyTcs = new();
    private readonly TaskCompletionSource<bool> _providersReadyTcs = new();
    protected readonly ILogger _logger;
    protected readonly IMessagingInfrastructure _messagingInfrastructure;
    protected readonly IMessageSubscriber _messageSubscriber;
    private readonly string _serviceName;
    public SystemReadinessService(
        ILogger<SystemReadinessService> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IOptions<SystemReadinessSettings> settings)
    {
        _logger = logger;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _serviceName = settings.Value.ServiceName;
    }

    public bool IsSystemReady => _systemReadyTcs.Task.IsCompleted;

    public bool IsProvidersReady => _providersReadyTcs.Task.IsCompleted;

    public async Task WaitForSystemReadyAsync(CancellationToken cancellationToken = default)
    {
        if (IsSystemReady)
            return;
        
        try
        {
            await _systemReadyTcs.Task.WaitAsync(cancellationToken);
            _logger.LogInformation("System is ready!");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("System readiness wait was cancelled");
        }
    }

    public async Task WaitForProvidersReadyAsync(CancellationToken cancellationToken = default)
    {
        if (IsProvidersReady)
            return;
        
        try
        {
            await _providersReadyTcs.Task.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Providers readiness wait was cancelled");
        }
    }

    public void MarkSystemReady()
    {
        if (!_systemReadyTcs.Task.IsCompleted)
        {
            _systemReadyTcs.TrySetResult(true);
        }
    }

    public void MarkProvidersReady()
    {
        if (!_providersReadyTcs.Task.IsCompleted)
        {
            _providersReadyTcs.TrySetResult(true);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
