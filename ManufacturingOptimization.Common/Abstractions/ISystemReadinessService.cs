namespace ManufacturingOptimization.Common.Abstractions;

/// <summary>
/// Service that coordinates system startup and blocks operations until all services are ready.
/// Also tracks provider readiness (all providers registered).
/// </summary>
public interface ISystemReadinessService
{
    /// <summary>
    /// Wait until the system is fully ready (all required services reported ready).
    /// </summary>
    Task WaitForSystemReadyAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Wait until all providers have registered and are ready.
    /// </summary>
    Task WaitForProvidersReadyAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Marks the system as ready. Called when SystemReadyEvent is received.
    /// </summary>
    void MarkSystemReady();
    
    /// <summary>
    /// Marks providers as ready. Called when AllProvidersRegisteredEvent is received.
    /// </summary>
    void MarkProvidersReady();
    
    /// <summary>
    /// Checks if the system is ready without blocking.
    /// </summary>
    bool IsSystemReady { get; }
    
    /// <summary>
    /// Checks if all providers are ready without blocking.
    /// </summary>
    bool IsProvidersReady { get; }
}
