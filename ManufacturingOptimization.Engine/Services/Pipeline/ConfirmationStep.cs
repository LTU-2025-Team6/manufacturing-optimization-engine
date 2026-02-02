using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Common.Models.Exceptions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Confirms accepted proposals with providers after strategy selection.
/// Sends final confirmation to providers that their proposals have been selected.
/// Works with ProcessStepEntity.
/// </summary>
public class ConfirmationStep : IWorkflowStep
{
    private readonly IMessagePublisher _messagePublisher;

    public ConfirmationStep(IMessagePublisher messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }

    public string Name => "Process Confirmation";

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        if (context.Plan.SelectedStrategy == null)
            throw new OptimizationException("No strategy selected for confirmation. Please select a strategy before proceeding.");

        var errors = new List<string>();
        var confirmationTasks = context.Plan.SelectedStrategy.Steps.Select(step => ConfirmWithProviderAsync(step, errors));
        await Task.WhenAll(confirmationTasks);

        if (errors.Any())
            throw new OptimizationException($"Confirmation failed: {string.Join("; ", errors)}");

        context.Plan.ConfirmedAt = DateTime.UtcNow;
        context.Plan.Status = OptimizationPlanStatus.Confirmed;

        _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanUpdated, new OptimizationPlanUpdatedEvent
        {
            Plan = context.Plan
        });
    }

    private async Task ConfirmWithProviderAsync(ProcessStepModel step, List<string> errors)
    {
        try
        {
            if (step.AllocatedSchedule == null)
                throw new OptimizationException($"No allocated slot found for step {step.Process} with provider {step.SelectedProviderName}.");

            var confirmation = new ConfirmProcessProposalCommand
            {
                ProposalId = step.ProposalId,
                SelectedSchedule = step.AllocatedSchedule
            };

            var response = await _messagePublisher.RequestReplyAsync<ProcessProposalReviewedEvent>(
                Exchanges.Process,
                $"process.confirm.{step.SelectedProviderId}",
                confirmation,
                TimeSpan.FromSeconds(10));

            if (response == null)
                throw new OptimizationException($"Provider {step.SelectedProviderName} did not respond to confirmation request within timeout. Process: {step.Process}");

            if (!response.IsAccepted)
                throw new OptimizationException($"Provider {step.SelectedProviderName} declined confirmation for {step.Process}. Reason: {response.DeclineReason}");
        }
        catch (OptimizationException ex)
        {
            errors.Add(ex.Message);
        }
        catch (Exception ex)
        {
            errors.Add($"Provider {step.SelectedProviderName} confirmation failed for {step.Process}: {ex.Message}");
        }
    }
}
