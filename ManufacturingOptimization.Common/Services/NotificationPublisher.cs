using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Common.Enums;

namespace ManufacturingOptimization.Common.Services;

/// <summary>
/// Helper service for publishing notifications.
/// All notification text and logic is centralized here.
/// </summary>
public class NotificationPublisher : INotificationPublisher
{
    private readonly IMessagePublisher _messagePublisher;

    public NotificationPublisher(IMessagePublisher messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }

    // === System Startup ===

    public void NotifySystemReady()
    {
        Publish(
            "System Ready",
            "All services are ready and system can process requests",
            NotificationType.Success,
            "System"
        );
    }

    // === Provider Lifecycle ===

    public void NotifyStartingAllProviders()
    {
        Publish(
            "Starting All Providers",
            "Initiating startup sequence for all provider services",
            NotificationType.Info,
            "Gateway"
        );
    }

    public void NotifyProviderStarted(string providerName)
    {
        Publish(
            $"Provider Started",
            $"Provider {providerName} has started and is ready to process requests",
            NotificationType.Success,
            providerName
        );
    }

    public void NotifyAllProvidersStarted()
    {
        Publish(
            "All Providers Started",
            "All provider services are running and ready to receive requests",
            NotificationType.Success,
            "Gateway"
        );
    }

    public void NotifyStoppingAllProviders()
    {
        Publish(
            "Stopping All Providers",
            "Initiating shutdown sequence for all provider services",
            NotificationType.Info,
            "Gateway"
        );
    }

    public void NotifyProviderStopped(Guid providerId)
    {
        Publish(
            "Provider Stopped",
            $"Provider {providerId} has been stopped",
            NotificationType.Success,
            "Gateway"
        );
    }

    public void NotifyAllProvidersStopped()
    {
        Publish(
            "All Providers Stopped",
            "All provider services have been stopped",
            NotificationType.Success,
            "Gateway"
        );
    }

    // === Optimization Flow ===

    public void NotifyOptimizationStarted(Guid planId)
    {
        Publish(
            "Optimization Started",
            $"Optimization pipeline started for plan {planId}",
            NotificationType.Info,
            "Engine"
        );
    }

    public void NotifyOptimizationCompleted(Guid planId)
    {
        Publish(
            "Optimization Completed",
            $"Plan {planId} has been successfully optimized and finalized",
            NotificationType.Success,
            "Engine"
        );
    }

    public void NotifyOptimizationFailed(Guid planId, string error)
    {
        Publish(
            "Optimization Failed",
            $"Plan {planId} failed: {error}",
            NotificationType.Error,
            "Engine"
        );
    }

    // === Pipeline Steps ===

    public void NotifyOptimizationStepStarted(string stepName, Guid planId)
    {
        Publish(
            $"Optimization Step Started: {stepName}",
            $"{stepName} step started for plan {planId}",
            NotificationType.Info,
            "Engine"
        );
    }

    // === Plan Updates ===

    public void NotifyStrategiesGenerated(Guid planId, int strategyCount)
    {
        Publish(
            "Strategies Generated",
            $"Plan {planId}: {strategyCount} optimization strategies have been generated and are ready for selection",
            NotificationType.Success,
            "Gateway"
        );
    }

    public void NotifyStrategySelected(Guid planId, Guid strategyId)
    {
        Publish(
            "Strategy Selected",
            $"Plan {planId}: Strategy {strategyId} has been selected",
            NotificationType.Info,
            "Gateway"
        );
    }

    public void NotifyOptimizationPlanUpdated(Guid planId, string status)
    {
        Publish(
            $"Plan Updated: {status}",
            $"Plan {planId} has been updated",
            NotificationType.Info,
            "Gateway"
        );
    }

    public void NotifyOptimizationPlanConfirmed(Guid planId)
    {
        Publish(
            "Plan Confirmed",
            $"Plan {planId} has been confirmed and is ready for execution",
            NotificationType.Success,
            "Gateway"
        );
    }

    // === Provider Operations ===

    public void NotifyProviderEstimatedProposal(string name, Guid planId, ProcessType process, bool accepted, string? declineReason)
    {
        var status = accepted ? "accepted" : "declined";
        var reason = !accepted && !string.IsNullOrEmpty(declineReason) ? $" (Reason: {declineReason})" : "";
        Publish(
            "Provider Estimated Proposal",
            $"Provider {name} {status} proposal for plan {planId}, process: {process}{reason}",
            accepted ? NotificationType.Success : NotificationType.Warning,
            name
        );
    }

    public void NotifyProviderCompletedConfirmationRequest(string name, Guid proposalId, Guid id, bool isAccepted, string? declineReason)
    {
        var status = isAccepted ? "confirmed" : "declined";
        var reason = !isAccepted && !string.IsNullOrEmpty(declineReason) ? $" (Reason: {declineReason})" : "";
        Publish(
            "Provider Completed Confirmation",
            $"Provider {name} {status} confirmation request {id} for proposal {proposalId}{reason}",
            isAccepted ? NotificationType.Success : NotificationType.Warning,
            name
        );
    }

    public void NotifyProviderCompletedScheduleRequest(string name, DateTime start, DateTime end)
    {
        Publish(
            "Provider Completed Schedule Request",
            $"Provider {name} completed schedule request for period {start:yyyy-MM-dd} to {end:yyyy-MM-dd}",
            NotificationType.Success,
            name
        );
    }

    public void NotifyProviderCompletedExecutionDetailsRequest(string name, Guid executionId)
    {
        Publish(
            "Provider Completed Execution Details Request",
            $"Provider {name} sent execution details for execution {executionId}",
            NotificationType.Success,
            name
        );
    }

    public void NotifyProviderStartedExecution(string providerName, Guid executionId, ProcessType process)
    {
        Publish(
            "Provider Started Execution",
            $"{providerName} started {process} execution {executionId}",
            NotificationType.Info,
            providerName
        );
    }

    public void NotifyProviderCompletedExecution(string providerName, Guid executionId, ProcessType process, bool success)
    {
        Publish(
            success ? "Provider Completed Execution" : "Provider Failed Execution",
            success 
                ? $"{providerName} completed {process} execution {executionId}"
                : $"{providerName} failed {process} execution {executionId}",
            success ? NotificationType.Success : NotificationType.Error,
            providerName
        );
    }

    public void NotifyProviderUpdated(string name)
    {
        Publish(
            "Provider Updated",
            $"Provider {name} has been updated successfully",
            NotificationType.Success,
            name
        );
    }

    // === Demo Data Generation ===

    public void NotifyDemoDataGenerationStarted(string providerName, Guid providerId)
    {
        Publish(
            "Demo Data Generation Started",
            $"Starting demo data generation for provider {providerName} (ID: {providerId})",
            NotificationType.Info,
            providerName
        );
    }

    public void NotifyDemoDataGenerationCompleted(string providerName, Guid providerId)
    {
        Publish(
            "Demo Data Generated",
            $"Demo data successfully generated for provider {providerName} (ID: {providerId})",
            NotificationType.Success,
            providerName
        );
    }

    public void NotifyDemoDataGenerationFailed(string providerName, Guid providerId, string error)
    {
        Publish(
            "Demo Data Generation Failed",
            $"Failed to generate demo data for provider {providerName} (ID: {providerId}): {error}",
            NotificationType.Error,
            providerName
        );
    }
    // === Process Cancellation ===

    public void NotifyProviderCompletedCancellationRequest(string name, Guid proposalId, bool success, string? errorMessage)
    {
        var status = success ? "successfully cancelled" : "failed to cancel";
        var error = !success && !string.IsNullOrEmpty(errorMessage) ? $" (Error: {errorMessage})" : "";
        Publish(
            success ? "Provider Cancelled Proposal" : "Provider Cancellation Failed",
            $"Provider {name} {status} proposal {proposalId}{error}",
            success ? NotificationType.Success : NotificationType.Error,
            name
        );
    }

    public void NotifyOptimizationPlanCancelled(Guid planId)
    {
        Publish(
            "Plan Cancelled",
            $"Optimization plan {planId} has been cancelled and reverted to Ready status",
            NotificationType.Warning,
            "Gateway"
        );
    }

    public void NotifyOptimizationPlanDeleted(Guid planId)
    {
        Publish(
            "Plan Deleted",
            $"Optimization plan {planId} has been deleted",
            NotificationType.Warning,
            "Gateway"
        );
    }

    public void NotifyExecutionStepStarted(Guid planId, string processName, string providerName, int stepNumber)
    {
        Publish(
            "▶️ Execution Started",
            $"Step {stepNumber}: {processName} execution started at {providerName}",
            NotificationType.Info,
            "Gateway"
        );
    }

    public void NotifyExecutionStepCompleted(Guid planId, string processName, string providerName, int stepNumber)
    {
        Publish(
            "✅ Execution Completed",
            $"Step {stepNumber}: {processName} execution completed successfully at {providerName}",
            NotificationType.Success,
            "Gateway"
        );
    }

    public void NotifyExecutionStepFailed(Guid planId, string processName, string providerName, int stepNumber, string reason)
    {
        Publish(
            "❌ Execution Failed",
            $"Step {stepNumber}: {processName} execution failed at {providerName}. Reason: {reason}",
            NotificationType.Error,
            "Gateway"
        );
    }

    // === Private Helper ===

    private void Publish(string title, string message, NotificationType type, string source)
    {
        var command = new CreateNotificationCommand
        {
            Title = title,
            Message = message,
            Type = type,
            Source = source
        };

        _messagePublisher.Publish(Exchanges.Notification, NotificationRoutingKeys.CreateNotification, command);
    }

    public void NotifyProviderSimulationTimeChanged(string name, DateTime simulatedUtcNow, double speedMultiplier)
    {
        var formattedTime = simulatedUtcNow.ToString("HH:mm dd-MM-yy");
        Publish(
            $"Simulation Time Changed: {formattedTime}, ×{speedMultiplier}",
            $"Provider {name} updated simulation time to {simulatedUtcNow:O} with speed multiplier ×{speedMultiplier}",
            NotificationType.Info,
            name
        );
    }
}
