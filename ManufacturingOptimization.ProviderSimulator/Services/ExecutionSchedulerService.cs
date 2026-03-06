using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Enums;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.ProviderSimulator.Abstractions;

namespace ManufacturingOptimization.ProviderSimulator.Services;

/// <summary>
/// Background service that polls execution schedule and automatically starts/completes executions
/// based on simulation time. Uses short polling intervals to respond to time changes quickly.
/// </summary>
public class ExecutionSchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IProviderSimulationContext _providerContext;
    private readonly ISimulationClock _clock;
    private readonly IMessagePublisher _publisher;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly ILogger<ExecutionSchedulerService> _logger;

    /// <summary>
    /// How many simulation-minutes past the scheduled start before marking as failed overdue.
    /// Started executions have a grace period before being marked overdue.
    /// </summary>
    private const int OverdueToleranceMinutes = 60;

    /// <summary>
    /// Real-time polling interval (5 seconds).
    /// </summary>
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

    public ExecutionSchedulerService(
        IServiceProvider serviceProvider,
        IProviderSimulationContext providerContext,
        ISimulationClock clock,
        IMessagePublisher publisher,
        INotificationPublisher notificationPublisher,
        ILogger<ExecutionSchedulerService> logger)
    {
        _serviceProvider = serviceProvider;
        _providerContext = providerContext;
        _clock = clock;
        _publisher = publisher;
        _notificationPublisher = notificationPublisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        if (stoppingToken.IsCancellationRequested)
            return;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingExecutionsAsync();
                await ProcessInProgressExecutionsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Provider}] Error in ExecutionScheduler", 
                    _providerContext.Provider.Name);
            }

            // Short polling interval - responds quickly to time changes
            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    /// <summary>
    /// Check pending executions and start or fail them based on simulation time.
    /// Only processes executions for this provider.
    /// </summary>
    private async Task ProcessPendingExecutionsAsync()
    {
        var now = _clock.UtcNow;
        var speedMultiplier = _clock.SpeedMultiplier;
        
        // Dynamic overdue tolerance: increase threshold for high-speed simulations
        // At 1x: 60 minutes, at 100x: 120 minutes, at 5000x: 600 minutes
        var dynamicTolerance = OverdueToleranceMinutes * Math.Max(1, speedMultiplier / 100.0);

        using var scope = _serviceProvider.CreateScope();
        var executionRepo = scope.ServiceProvider.GetRequiredService<IExecutionRepository>();

        // Get pending executions for this provider only
        var pendingExecutions = await executionRepo.GetExecutionsByProviderAndStatusAsync(
            _providerContext.Provider.Id, 
            StepExecutionStatus.Pending);

        if (!pendingExecutions.Any())
            return;

        foreach (var execution in pendingExecutions)
        {
            if (!execution.ScheduleSegments.Any())
                continue;

            var firstStart = execution.ScheduleSegments.Min(s => s.StartTime);
            var lastEnd = execution.ScheduleSegments.Max(s => s.EndTime);

            // Not yet time to start
            if (firstStart > now)
                continue;

            // The execution window is completely over (past the last segment end + tolerance).
            // This covers both normal overdue AND large clock jumps that skip past the whole window.
            // We deliberately do NOT fail just because (now > firstStart + tolerance) — a user might
            // jump the simulation clock forward several hours, landing inside the execution window.
            // In that case we still want to start the execution rather than immediately failing it.
            var windowClosedThreshold = lastEnd.AddMinutes(dynamicTolerance);

            if (now > windowClosedThreshold)
            {
                // Entire execution window has passed — truly missed
                execution.Status = StepExecutionStatus.Failed;
                execution.CompletedAt = now;
                await executionRepo.UpdateAsync(execution);
                await executionRepo.SaveChangesAsync();
                
                _notificationPublisher.NotifyProviderCompletedExecution(
                    _providerContext.Provider.Name, execution.Id, execution.Proposal.Process, success: false);
                
                _publisher.Publish(
                    Exchanges.Process,
                    ProcessRoutingKeys.ExecutionCompleted,
                    new ProcessExecutionCompletedEvent
                    {
                        PlanId = execution.Proposal.PlanId,
                        ProposalId = execution.ProposalId,
                        ProviderId = _providerContext.Provider.Id,
                        ProcessName = execution.Proposal.Process.ToString(),
                        Success = false,
                        FailureReason = $"Execution window closed — scheduled {firstStart:O}–{lastEnd:O}, current time {now:O}",
                        CompletedAt = now
                    });
            }
            else
            {
                // Start execution — either on time or late due to a simulation clock jump,
                // but the execution window (up to lastEnd + tolerance) is still open.
                execution.Status = StepExecutionStatus.InProgress;
                execution.StartedAt = now;
                await executionRepo.UpdateAsync(execution);
                await executionRepo.SaveChangesAsync();
                
                _notificationPublisher.NotifyProviderStartedExecution(
                    _providerContext.Provider.Name, execution.Id, execution.Proposal.Process);
                
                _publisher.Publish(
                    Exchanges.Process,
                    ProcessRoutingKeys.ExecutionStarted,
                    new ProcessExecutionStartedEvent
                    {
                        PlanId = execution.Proposal.PlanId,
                        ProposalId = execution.ProposalId,
                        ProviderId = _providerContext.Provider.Id,
                        ProcessName = execution.Proposal.Process.ToString(),
                        StartedAt = now
                    });
            }
        }
    }

    /// <summary>
    /// Check in-progress executions and complete them when schedule ends.
    /// Only processes executions for this provider.
    /// </summary>
    private async Task ProcessInProgressExecutionsAsync()
    {
        var now = _clock.UtcNow;
        var speedMultiplier = _clock.SpeedMultiplier;
        
        // Dynamic overdue tolerance for completion
        var dynamicTolerance = OverdueToleranceMinutes * Math.Max(1, speedMultiplier / 100.0);

        using var scope = _serviceProvider.CreateScope();
        var executionRepo = scope.ServiceProvider.GetRequiredService<IExecutionRepository>();

        // Get in-progress executions for this provider only
        var inProgressExecutions = await executionRepo.GetExecutionsByProviderAndStatusAsync(
            _providerContext.Provider.Id, 
            StepExecutionStatus.InProgress);

        if (!inProgressExecutions.Any())
            return;

        foreach (var execution in inProgressExecutions)
        {
            if (!execution.ScheduleSegments.Any())
                continue;

            var firstStart = execution.ScheduleSegments.Min(s => s.StartTime);
            var lastEnd = execution.ScheduleSegments.Max(s => s.EndTime);

            // Still in progress
            if (lastEnd > now)
                continue;

            var overdueEndThreshold = lastEnd.AddMinutes(dynamicTolerance);

            if (now > overdueEndThreshold)
            {
                // Overdue to complete - mark as failed
                execution.Status = StepExecutionStatus.Failed;
                execution.CompletedAt = now;
                await executionRepo.UpdateAsync(execution);
                await executionRepo.SaveChangesAsync();
                
                _notificationPublisher.NotifyProviderCompletedExecution(
                    _providerContext.Provider.Name, execution.Id, execution.Proposal.Process, success: false);
                
                _publisher.Publish(
                    Exchanges.Process,
                    ProcessRoutingKeys.ExecutionCompleted,
                    new ProcessExecutionCompletedEvent
                    {
                        PlanId = execution.Proposal.PlanId,
                        ProposalId = execution.ProposalId,
                        ProviderId = _providerContext.Provider.Id,
                        ProcessName = execution.Proposal.Process.ToString(),
                        Success = false,
                        FailureReason = $"Execution completion overdue — expected end {lastEnd:O}",
                        CompletedAt = now
                    });
            }
            else
            {
                // Complete successfully
                execution.Status = StepExecutionStatus.Completed;
                execution.CompletedAt = now;
                await executionRepo.UpdateAsync(execution);
                await executionRepo.SaveChangesAsync();

                _notificationPublisher.NotifyProviderCompletedExecution(
                    _providerContext.Provider.Name, execution.Id, execution.Proposal.Process, success: true);

                _publisher.Publish(Exchanges.Process, ProcessRoutingKeys.ExecutionCompleted,
                    new ProcessExecutionCompletedEvent
                    {
                        PlanId = execution.Proposal.PlanId,
                        ProposalId = execution.ProposalId,
                        ProviderId = _providerContext.Provider.Id,
                        ProcessName = execution.Proposal.Process.ToString(),
                        Success = true,
                        CompletedAt = now
                    });
            }
        }
    }
}
