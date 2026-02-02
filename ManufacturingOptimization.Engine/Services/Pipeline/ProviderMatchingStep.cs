using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Common.Models.Exceptions;
using ManufacturingOptimization.Engine.Models;
using ManufacturingOptimization.Engine.Models.OptimizationStep;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Step 2: Provider Matching
/// For each process step, finds providers with the required capability.
/// Works with ProviderEntity.
/// </summary>
public class ProviderMatchingStep : IWorkflowStep
{
    private readonly IProviderRepository _providerRepository;
    private readonly IMessagePublisher _messagePublisher;

    public string Name => "Provider Matching";

    public ProviderMatchingStep(IProviderRepository providerRepository, IMessagePublisher messagePublisher)
    {
        _providerRepository = providerRepository;
        _messagePublisher = messagePublisher;
    }

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
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
                .Where(pc => MeetsTechnicalRequirements(pc.ProviderEntity, context.Request))
                .Select(pc => new MatchedProvider
                {
                    ProviderId = pc.ProviderEntity.Id,
                    ProviderName = pc.ProviderEntity.Name
                })
                .ToList();

            processStep.MatchedProviders = matchedProviders;

            if (processStep.MatchedProviders.Count == 0)
                throw new OptimizationException($"No providers available for process '{processStep.Process}' (step {processStep.StepNumber}) with required technical capabilities. Please ensure providers are registered and meet the specifications.");
        }
    }

    private static bool MeetsTechnicalRequirements(ProviderEntity provider, OptimizationRequestModel request)
    {
        if (provider.TechnicalCapabilities == null)
            return false;

        // Provider must be able to handle the motor's power and size
        bool canHandlePower = provider.TechnicalCapabilities.Power == 0 || provider.TechnicalCapabilities.Power >= request.MotorSpecs.PowerKW;
        bool canHandleSize = provider.TechnicalCapabilities.AxisHeight == 0 || provider.TechnicalCapabilities.AxisHeight >= request.MotorSpecs.AxisHeightMM;
        
        return canHandlePower && canHandleSize;
    }
}
