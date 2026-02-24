using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Common.Messaging;

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

    public void NotifyStartingProvider(Guid providerId)
    {
        Publish(
            "Starting Provider",
            $"Starting provider {providerId}",
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

    public void NotifyStoppingProvider(Guid providerId)
    {
        Publish(
            "Stopping Provider",
            $"Stopping provider {providerId}",
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

    public void NotifyOptimizationPlanUpdated(Guid planId)
    {
        Publish(
            "Plan Updated",
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

    public void NotifyProviderReceivedProposal(string name, Guid planId, ProcessType process)
    {
        Publish(
            "Provider Received Proposal",
            $"Provider {name} received a proposal for plan {planId}, process: {process}",
            NotificationType.Info,
            name
        );
    }

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

    public void NotifyProviderReceivedConfirmationRequest(string name, Guid proposalId, Guid id)
    {
        Publish(
            "Provider Received Confirmation Request",
            $"Provider {name} received confirmation request {id} for proposal {proposalId}",
            NotificationType.Info,
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

    public void NotifyProviderReceivedScheduleRequest(string name, DateTime start, DateTime end)
    {
        Publish(
            "Provider Received Schedule Request",
            $"Provider {name} received schedule request for period {start:yyyy-MM-dd} to {end:yyyy-MM-dd}",
            NotificationType.Info,
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

    public void NotifyProviderUpdateRequested(string name)
    {
        Publish(
            "Provider Update Requested",
            $"Update requested for provider {name}",
            NotificationType.Info,
            name
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
}
