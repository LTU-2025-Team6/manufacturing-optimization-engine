using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Common.Models.Exceptions;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;
using ManufacturingOptimization.Engine.Models.OptimizationStep;
using System.Diagnostics;
using System.Numerics;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Proposes processes to matched providers for preliminary acceptance.
/// Providers can accept with estimates or decline the proposal.
/// This step supports both proposal-based and direct estimation flows.
/// </summary>
public class EstimationStep : IWorkflowStep
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly IAsyncAwaiter _asyncAwaiter;

    public EstimationStep(
        IMessagePublisher messagePublisher,
        IAsyncAwaiter asyncAwaiter)
    {
        _messagePublisher = messagePublisher;
        _asyncAwaiter = asyncAwaiter;
    }

    public string Name => "Proposal & Estimation";

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        context.Plan.Status = OptimizationPlanStatus.EstimatingCosts;
        _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanUpdated, new OptimizationPlanUpdatedEvent
        {
            Plan = context.Plan
        });

        var errors = new List<string>();
        
        foreach (var step in context.ProcessSteps)
        {
            var proposalTasks = step.MatchedProviders
                .Select(provider => ProposeToProviderAsync(context, step, provider, errors))
                .ToArray();

            var results = await Task.WhenAll(proposalTasks);
            
            // Remove providers that declined or failed
            for (int i = step.MatchedProviders.Count - 1; i >= 0; i--)
            {
                if (!results[i])
                    step.MatchedProviders.RemoveAt(i);
            }
        }
        
        if (errors.Any())
            throw new OptimizationException($"Estimation failed: {string.Join("; ", errors)}");
    }

    private async Task<bool> ProposeToProviderAsync(WorkflowContext context, WorkflowProcessStep step, MatchedProvider provider, List<string> errors)
    {
        try
        {
            var response = await _asyncAwaiter.AwaitAsync(new AwaitScenario<ProcessProposalEstimatedEvent>
            {
                Exchange = Exchanges.Process,
                RoutingKey = $"{ProcessRoutingKeys.Estimated}.{provider.ProviderId}",
                Timeout = TimeSpan.FromSeconds(10),
                Match = evt => true,
                BeforeAwait = () =>
                    _messagePublisher.Publish(Exchanges.Process, $"{ProcessRoutingKeys.Propose}.{provider.ProviderId}", new ProposeProcessToProviderCommand
                    {
                        PlanId = context.Plan.Id,
                        ProviderId = provider.ProviderId,
                        Process = step.Process,
                        MotorSpecs = context.Request.MotorSpecs,
                        RequestedTimeWindow = context.Request.Constraints.TimeWindow
                    })
            });

            if (!response.Accepted)
                return false; // Provider declined

            provider.ProposalId = response.ProposalId
                ?? throw new OptimizationException($"Provider {provider.ProviderName} accepted proposal but did not provide a proposal id");
            provider.Estimate = response.Estimate
                ?? throw new OptimizationException($"Provider {provider.ProviderName} accepted proposal but did not provide an estimate");
            provider.Schedule = response.Schedule
                ?? throw new OptimizationException($"Provider {provider.ProviderName} accepted proposal but did not provide a schedule");
            
            return true; // Success
        }
        catch (OptimizationException ex)
        {
            errors.Add(ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            errors.Add($"Provider {provider.ProviderName} estimation failed for {step.Process}: {ex.Message}");
            return false;
        }
    }
}
