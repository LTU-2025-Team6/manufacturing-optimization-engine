using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Contracts;
using ManufacturingOptimization.Common.Enums;
using ManufacturingOptimization.Common.Exceptions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.OptimizationPipeline;

/// <summary>
/// Step 2: Provider Matching
/// For each process step, finds providers with the required capability.
/// Works with ProviderEntity.
/// </summary>
public class ProviderMatchingStep : IWorkflowStep
{
    private readonly IProviderRepository _providerRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly INotificationPublisher _notificationPublisher;

    public ProviderMatchingStep(
        IProviderRepository providerRepository,
        IMessagePublisher messagePublisher,
        INotificationPublisher notificationPublisher)
    {
        _providerRepository = providerRepository;
        _messagePublisher = messagePublisher;
        _notificationPublisher = notificationPublisher;
    }

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        _notificationPublisher.NotifyOptimizationStepStarted("Provider Matching", context.Plan.Id);

        context.Plan.Status = OptimizationPlanStatus.MatchingProviders;

        _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanUpdated, new OptimizationPlanUpdatedEvent
        {
            Plan = context.Plan
        });

        foreach (var processStep in context.ProcessSteps)
        {
            // Find providers that can perform this process
            var providersWithCapabilities = await _providerRepository.FindByProcess(processStep.Process);
            
            // Filter by technical requirements
            var matchedProviders = providersWithCapabilities
                .Where(pc => MeetsTechnicalRequirements(pc.Provider, context.Request))
                .Select(pc => new MatchedProvider
                {
                    ProviderId = pc.Provider.Id,
                    ProviderName = pc.Provider.Name
                })
                .ToList();

            processStep.MatchedProviders = matchedProviders;

            if (processStep.MatchedProviders.Count == 0)
                throw new OptimizationException($"No providers available for process '{processStep.Process}' (step {processStep.StepNumber}) with required technical capabilities. Please ensure providers are registered and meet the specifications.");
        }
    }

    private static bool MeetsTechnicalRequirements(ProviderModel provider, OptimizationRequestModel request)
    {
        if (provider.TechnicalCapabilities == null)
            return false;

        // Provider must be able to handle the motor's power and size
        bool canHandlePower = provider.TechnicalCapabilities.Power == 0 || provider.TechnicalCapabilities.Power >= request.MotorSpecs.PowerKW;
        bool canHandleSize = provider.TechnicalCapabilities.AxisHeight == 0 || provider.TechnicalCapabilities.AxisHeight >= request.MotorSpecs.AxisHeightMM;
        
        return canHandlePower && canHandleSize;
    }
}
