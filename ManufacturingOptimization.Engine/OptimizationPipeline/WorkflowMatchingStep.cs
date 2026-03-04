using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Enums;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.OptimizationPipeline;

/// <summary>
/// Step 1: Workflow Matching
/// Determines workflow type (Upgrade vs Refurbish) and generates process steps.
/// </summary>
public class WorkflowMatchingStep : IWorkflowStep
{
    private readonly IMessagePublisher _messagePublisher;
    private readonly INotificationPublisher _notificationPublisher;

    public WorkflowMatchingStep(
        IMessagePublisher messagePublisher,
        INotificationPublisher notificationPublisher)
    {
        _messagePublisher = messagePublisher;
        _notificationPublisher = notificationPublisher;
    }

    public Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        _notificationPublisher.NotifyOptimizationStepStarted("Workflow Matching", context.Plan.Id);

        context.Plan.Status = OptimizationPlanStatus.MatchingWorkflow;
        _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanUpdated, new OptimizationPlanUpdatedEvent
        {
            Plan = context.Plan
        });

        var currentEfficiency = context.Request.MotorSpecs.CurrentEfficiency;
        var targetEfficiency = context.Request.MotorSpecs.TargetEfficiency;
        
        bool isUpgrade = targetEfficiency > currentEfficiency;
        context.WorkflowType = isUpgrade ? "Upgrade" : "Refurbish";
        
        // Generate process steps
        context.ProcessSteps = isUpgrade 
            ? CreateUpgradeSteps() 
            : CreateRefurbishSteps();
        
        return Task.CompletedTask;
    }

    private static List<WorkflowProcessStep> CreateUpgradeSteps()
    {
        return new List<WorkflowProcessStep>
        {
            new() { StepNumber = 1, Process = ProcessType.Cleaning },
            new() { StepNumber = 2, Process = ProcessType.Disassembly },
            new() { StepNumber = 3, Process = ProcessType.Redesign },
            new() { StepNumber = 4, Process = ProcessType.Turning },
            new() { StepNumber = 6, Process = ProcessType.PartSubstitution },
            new() { StepNumber = 7, Process = ProcessType.Reassembly },
            new() { StepNumber = 8, Process = ProcessType.Certification }
        };
    }

    private static List<WorkflowProcessStep> CreateRefurbishSteps()
    {
        return new List<WorkflowProcessStep>
        {
            new() { StepNumber = 1, Process = ProcessType.Cleaning },
            new() { StepNumber = 2, Process = ProcessType.Disassembly },
            new() { StepNumber = 3, Process = ProcessType.PartSubstitution },
            new() { StepNumber = 4, Process = ProcessType.Reassembly },
            new() { StepNumber = 5, Process = ProcessType.Certification }
        };
    }
}
