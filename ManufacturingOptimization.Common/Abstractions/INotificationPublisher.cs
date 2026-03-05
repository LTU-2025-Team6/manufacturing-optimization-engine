using ManufacturingOptimization.Common.Enums;

namespace ManufacturingOptimization.Common.Abstractions;

/// <summary>
/// Helper service for publishing system notifications.
/// All notification logic and text is centralized here.
/// </summary>
public interface INotificationPublisher
{
    void NotifySystemReady();


    void NotifyStartingAllProviders();
    void NotifyProviderStarted(string providerName);
    void NotifyAllProvidersStarted();

    void NotifyStoppingAllProviders();
    void NotifyProviderStopped(Guid providerId);
    void NotifyAllProvidersStopped();


    void NotifyOptimizationStarted(Guid planId);
    void NotifyOptimizationStepStarted(string stepName, Guid planId);
    void NotifyStrategiesGenerated(Guid planId, int strategyCount);
    void NotifyStrategySelected(Guid planId, Guid strategyId);
    void NotifyOptimizationCompleted(Guid planId);
    void NotifyOptimizationFailed(Guid planId, string error);
    void NotifyOptimizationPlanUpdated(Guid planId, string status);
    void NotifyOptimizationPlanConfirmed(Guid planId);


    void NotifyProviderEstimatedProposal(string name, Guid planId, ProcessType process, bool accepted, string? declineReason);
    void NotifyProviderCompletedConfirmationRequest(string name, Guid proposalId, Guid id, bool isAccepted, string? declineReason);
    void NotifyProviderCompletedScheduleRequest(string name, DateTime start, DateTime end);
    void NotifyProviderCompletedExecutionDetailsRequest(string name, Guid executionId);
    void NotifyProviderStartedExecution(string providerName, Guid executionId, ProcessType process);
    void NotifyProviderCompletedExecution(string providerName, Guid executionId, ProcessType process, bool success);
    void NotifyProviderUpdated(string name);
    void NotifyProviderSimulationTimeChanged(string name, DateTime simulatedUtcNow, double speedMultiplier);
    void NotifyProviderCompletedCancellationRequest(string name, Guid proposalId, bool success, string? errorMessage);


    void NotifyDemoDataGenerationStarted(string providerName, Guid providerId);
    void NotifyDemoDataGenerationCompleted(string providerName, Guid providerId);
    void NotifyDemoDataGenerationFailed(string providerName, Guid providerId, string error);


    void NotifyOptimizationPlanCancelled(Guid planId);
    void NotifyOptimizationPlanDeleted(Guid planId);

    void NotifyExecutionStepStarted(Guid planId, string processName, string providerName, int stepNumber);
    void NotifyExecutionStepCompleted(Guid planId, string processName, string providerName, int stepNumber);
    void NotifyExecutionStepFailed(Guid planId, string processName, string providerName, int stepNumber, string reason);
}
