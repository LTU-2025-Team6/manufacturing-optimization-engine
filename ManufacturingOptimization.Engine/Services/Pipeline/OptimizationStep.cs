using Google.OrTools.LinearSolver;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Common.Models.Exceptions;
using ManufacturingOptimization.Common.Models.Extensions;
using ManufacturingOptimization.Engine.Models;
using ManufacturingOptimization.Engine.Models.OptimizationStep;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Workflow step responsible for generating multiple optimization strategies
/// using Google OR-Tools.
/// </summary>
public sealed partial class OptimizationStep : IWorkflowStep
{
    private const int SlotGranularityMinutes = 60;

    private readonly IMessagePublisher _messagePublisher;

    public OptimizationStep(IMessagePublisher messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }

    /// All optimization priorities we want to generate strategies for.
    private static readonly OptimizationPriority[] Priorities =
    {
        OptimizationPriority.LowestCost,
        OptimizationPriority.FastestDelivery,
        OptimizationPriority.HighestQuality,
        OptimizationPriority.LowestEmissions
    };

    public string Name => "Optimization & Strategy Generation";

    /// <summary>
    /// Main workflow execution method.
    /// </summary>
    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        context.Plan.Status = OptimizationPlanStatus.GeneratingStrategies;
        _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanUpdated, new OptimizationPlanUpdatedEvent
        {
            Plan = context.Plan
        });

        // Ensure all process steps have at least one provider.
        // Optimization is impossible otherwise.
        ValidateProviders(context);
        
        PreprocessTimeSlots(context);

        foreach (var priority in Priorities)
        {
            // Run pure optimization calculation
            var result = await OptimizeForPriorityAsync(context, priority, cancellationToken);

            if (result == null)
                continue;

            // Convert calculation result into a domain strategy
            var strategy = CreateStrategy(priority, context, result);
            
            // Clamp schedules to actual working timeline (remove empty time before/after work)
            strategy.ClampSchedulesToWorkingTimeline();
            
            // Add to plan
            context.Plan.Strategies.Add(strategy);
        }

        if (context.Plan.Strategies.Count == 0)
            throw new OptimizationException("Failed to generate optimization strategies. No feasible solutions found for the given constraints.");
    }
    
    /// <summary>
    /// Preprocesses time slots for MIP optimization.
    /// Converts DateTime slots to hours relative to reference time.
    /// </summary>
    private static void PreprocessTimeSlots(WorkflowContext context)
    {
        var referenceTime = context.Request.Constraints.TimeWindow.StartTime;
        
        foreach (var step in context.ProcessSteps)
        {
            foreach (var provider in step.MatchedProviders)
            {
                var timeSlots = provider.Schedule.Segments.BuildPossibleWorkSlots(provider.Estimate.Duration, SlotGranularityMinutes);
                
                if (timeSlots.Count == 0)
                {
                    provider.IndexedSlots = new List<IndexedTimeSlot>();
                    continue;
                }

                provider.IndexedSlots = timeSlots
                    .Select((segments, index) =>
                    {
                        var workingSegments = segments.Where(s => s.SegmentType == SegmentType.WorkingTime).ToList();
                        
                        if (workingSegments.Count == 0)
                            return null;
                        
                        var startTime = workingSegments.Min(s => s.StartTime);
                        var endTime = workingSegments.Max(s => s.EndTime);
                        
                        return new IndexedTimeSlot
                        {
                            SlotIndex = index,
                            Slot = new ProviderScheduleModel
                            {
                                Segments = segments
                            },
                            StartTimeHours = (startTime - referenceTime).TotalHours,
                            EndTimeHours = (endTime - referenceTime).TotalHours
                        };
                    })
                    .Where(slot => slot != null)
                    .Cast<IndexedTimeSlot>()
                    .ToList();
            }
        }
    }

    /// <summary>
    /// Ensures that every workflow step has at least one matched provider.
    /// </summary>
    private static void ValidateProviders(WorkflowContext context)
    {
        if (context.ProcessSteps.Any(s => s.MatchedProviders.Count == 0))
        {
            throw new OptimizationException("Cannot optimize: some process steps have no available providers with required capabilities.");
        }
    }

    /// <summary>
    /// Runs optimization for a specific priority.
    /// </summary>
    private Task<OptimizationResult?> OptimizeForPriorityAsync(WorkflowContext context, OptimizationPriority priority, CancellationToken cancellationToken)
    {
        var weights = priority.GetWeights();

        // Create OR-Tools solver
        var solver = Solver.CreateSolver("SCIP");

        if (solver == null)
            throw new OptimizationException("Failed to initialize optimization solver. Please try again.");

        return Task.FromResult(OptimizeWithTimeSlots(context, weights, solver));
    }
    
    /// <summary>
    /// Optimizes with time slot constraints and sequential execution.
    /// Uses MIP with time variables and Big M constraints.
    /// </summary>
    private OptimizationResult? OptimizeWithTimeSlots(WorkflowContext context, OptimizationWeights weights, Solver solver)
    {
        // Create MIP variables: x[i,j,k], start[i], end[i]
        var variables = CreateMipVariables(solver, context);
        
        if (variables.Assignments.Count == 0)
        {
            // No feasible assignments possible
            return null;
        }

        // Add all MIP constraints
        AddMipConstraints(solver, context, variables);

        // Build objective function
        var objective = BuildMipObjective(solver, context, variables, weights);
        objective.SetMinimization();
        
        // Diagnostic: Check provider overlap across steps
        var providerIdsByStep = context.ProcessSteps
            .Select(s => s.MatchedProviders.Select(p => p.ProviderId).ToHashSet())
            .ToList();
        
        for (int i = 0; i < context.ProcessSteps.Count; i++)
        {
            var step = context.ProcessSteps[i];
            var totalSlots = step.MatchedProviders.Sum(p => p.IndexedSlots.Count);
            
            // Count how many OTHER steps share providers with this step
            int sharedProviderSteps = 0;
            for (int j = 0; j < providerIdsByStep.Count; j++)
            {
                if (i != j && providerIdsByStep[i].Overlaps(providerIdsByStep[j]))
                    sharedProviderSteps++;
            }
        }

        // Solve with timeout
        //solver.SetTimeLimit(60000); // 60 seconds

        var status = solver.Solve();
        
        if (status is not Solver.ResultStatus.OPTIMAL and not Solver.ResultStatus.FEASIBLE)
        {
            return null;
        }

        return ExtractMipResult(context, variables, objective, status);
    }

    /// <summary>
    /// Normalizes a value to 0..1 range based on min and max values.
    /// </summary>
    private static double Normalize(double value, double min, double max)
    {
        if (max <= min)
            return 0;

        return (value - min) / (max - min);
    }

    /// <summary>
    /// Builds a domain-level optimization strategy from calculation result.
    /// </summary>
    private OptimizationStrategyModel CreateStrategy(OptimizationPriority priority, WorkflowContext context, OptimizationResult result)
    {
        if (context.WorkflowType == null)
            throw new ArgumentNullException(nameof(result));

        var (name, description) = priority.GetStrategyNameAndDescription();
        var warrantyTerms = priority.GetWarrantyTerms(context.WorkflowType);

        return new OptimizationStrategyModel
        {
            Id = Guid.NewGuid(), // Important to assign Id here so that it correlates with one stored at Gateway
            PlanId = context.Plan.Id,
            StrategyName = name,
            Priority = priority,
            WorkflowType = context.WorkflowType,
            Steps = context.ProcessSteps.Select(step =>
            {
                var provider = result.SelectedProviders[step.StepNumber];
                var scheduledProcess = result.Timeline?.Processes.FirstOrDefault(p => p.StepNumber == step.StepNumber);

                return new ProcessStepModel
                {
                    StepNumber = step.StepNumber,
                    Process = step.Process,
                    SelectedProviderId = provider.ProviderId,
                    SelectedProviderName = provider.ProviderName,
                    AllocatedSchedule = scheduledProcess?.AllocatedSchedule != null 
                        ? new ProviderScheduleModel 
                        { 
                            Segments = scheduledProcess.AllocatedSchedule.Segments
                        } 
                        : null,
                    Estimate = new ProcessEstimateModel
                    {
                        Cost = provider.Estimate.Cost,
                        QualityScore = provider.Estimate.QualityScore,
                        EmissionsKgCO2 = provider.Estimate.EmissionsKgCO2,
                        
                    },
                    ProposalId = provider.ProposalId
                };
            }).ToList(),
            Metrics = result.Metrics,
            Warranty = warrantyTerms,
            Description = description
        };
    }
    
    #region MIP Time Slot Optimization Methods
    
    /// <summary>
    /// Creates MIP variables for time-aware optimization.
    /// </summary>
    private static MipVariables CreateMipVariables(Solver solver, WorkflowContext context)
    {
        var variables = new MipVariables();

        // Calculate window duration
        var windowDurationHours = (context.Request.Constraints.TimeWindow.EndTime - context.Request.Constraints.TimeWindow.StartTime).TotalHours;
        
        // 1. Create binary assignment variables x[i,j,k]
        for (int i = 0; i < context.ProcessSteps.Count; i++)
        {
            var step = context.ProcessSteps[i];
            
            for (int j = 0; j < step.MatchedProviders.Count; j++)
            {
                var provider = step.MatchedProviders[j];
                
                for (int k = 0; k < provider.IndexedSlots.Count; k++)
                {
                    var key = (i, j, k);
                    variables.Assignments[key] = solver.MakeBoolVar($"x_{i}_{j}_{k}");
                }
            }
        }
        
        // 2. Create continuous time variables start[i] and end[i]
        for (int i = 0; i < context.ProcessSteps.Count; i++)
        {
            variables.StartTimes[i] = solver.MakeNumVar(0, windowDurationHours, $"start_{i}");
            variables.EndTimes[i] = solver.MakeNumVar(0, windowDurationHours, $"end_{i}");
        }
        
        return variables;
    }
    
    /// <summary>
    /// Adds all MIP constraints for time-aware optimization.
    /// </summary>
    private void AddMipConstraints(Solver solver, WorkflowContext context, MipVariables variables)
    {
        AddMipOneProviderPerStepConstraints(solver, context, variables);
        AddSequentialConstraints(solver, context, variables);
        AddSimpleTimeSlotConstraints(solver, context, variables);
        AddDeadlineConstraints(solver, context, variables);
        AddMipBudgetConstraints(solver, context, variables);
    }
    
    /// <summary>
    /// Constraint: Each step must select exactly one provider and exactly one time slot.
    /// </summary>
    private static void AddMipOneProviderPerStepConstraints(Solver solver, WorkflowContext context, MipVariables variables)
    {
        for (int i = 0; i < context.ProcessSteps.Count; i++)
        {
            var step = context.ProcessSteps[i];
            
            // Exactly one slot must be selected across all providers
            var oneSlotConstraint = solver.MakeConstraint(1, 1, $"one_slot_{i}");
            
            for (int j = 0; j < step.MatchedProviders.Count; j++)
            {
                var numSlots = step.MatchedProviders[j].IndexedSlots.Count;
                
                for (int k = 0; k < numSlots; k++)
                {
                    oneSlotConstraint.SetCoefficient(variables.Assignments[(i, j, k)], 1);
                }
            }
        }
    }
    
    /// <summary>
    /// Constraint: Processes must execute sequentially (start[i+1] >= end[i]).
    /// </summary>
    private static void AddSequentialConstraints(Solver solver, WorkflowContext context, MipVariables variables)
    {
        for (int i = 0; i < context.ProcessSteps.Count - 1; i++)
        {
            // start[i+1] >= end[i]
            var constraint = solver.MakeConstraint(0, double.PositiveInfinity, $"sequential_{i}");
            constraint.SetCoefficient(variables.StartTimes[i + 1], 1);
            constraint.SetCoefficient(variables.EndTimes[i], -1);
        }
    }
    
    /// <summary>
    /// Simple time slot constraints without tightness - allows MIP more flexibility.
    /// </summary>
    private void AddSimpleTimeSlotConstraints(Solver solver, WorkflowContext context, MipVariables variables)
    {
        const double BigM = 100000;
        
        for (int i = 0; i < context.ProcessSteps.Count; i++)
        {
            var step = context.ProcessSteps[i];
            
            for (int j = 0; j < step.MatchedProviders.Count; j++)
            {
                var provider = step.MatchedProviders[j];
                
                for (int k = 0; k < provider.IndexedSlots.Count; k++)
                {
                    var slot = provider.IndexedSlots[k];
                    var x = variables.Assignments[(i, j, k)];
                    
                    // If slot selected: start[i] <= slot.start
                    var c1 = solver.MakeConstraint(
                        double.NegativeInfinity,
                        slot.StartTimeHours + BigM,
                        $"slot_start_{i}_{j}_{k}");
                    c1.SetCoefficient(variables.StartTimes[i], 1);
                    c1.SetCoefficient(x, BigM);
                    
                    // If slot selected: end[i] >= slot.end
                    var c2 = solver.MakeConstraint(
                        slot.EndTimeHours - BigM,
                        double.PositiveInfinity,
                        $"slot_end_{i}_{j}_{k}");
                    c2.SetCoefficient(variables.EndTimes[i], 1);
                    c2.SetCoefficient(x, -BigM);
                }
            }
        }
    }
    
    /// <summary>
    /// Constraint: Last process must end before deadline.
    /// </summary>
    private void AddDeadlineConstraints(Solver solver, WorkflowContext context, MipVariables variables)
    {
        var deadlineHours = (context.Request.Constraints.TimeWindow.EndTime - context.Request.Constraints.TimeWindow.StartTime).TotalHours;
        
        var lastStepIndex = context.ProcessSteps.Count - 1;
        solver.Add(variables.EndTimes[lastStepIndex] <= deadlineHours);
    }
    
    /// <summary>
    /// Constraint: Total cost must not exceed budget.
    /// </summary>
    private void AddMipBudgetConstraints(Solver solver, WorkflowContext context, MipVariables variables)
    {
        var constraints = context.Request.Constraints;

        if (!constraints.MaxBudget.HasValue)
            return;
        
        LinearExpr totalCostExpr = new LinearExpr();
        
        for (int i = 0; i < context.ProcessSteps.Count; i++)
        {
            var step = context.ProcessSteps[i];
            
            for (int j = 0; j < step.MatchedProviders.Count; j++)
            {
                var cost = (double)step.MatchedProviders[j].Estimate.Cost;
                
                for (int k = 0; k < step.MatchedProviders[j].IndexedSlots.Count; k++)
                {
                    totalCostExpr += cost * variables.Assignments[(i, j, k)];
                }
            }
        }
        
        solver.Add(totalCostExpr <= (double)constraints.MaxBudget.Value);
    }
    
    /// <summary>
    /// Builds MIP objective function with time-aware metrics.
    /// </summary>
    private Objective BuildMipObjective(Solver solver, WorkflowContext context, MipVariables variables, OptimizationWeights weights)
    {
        var objective = solver.Objective();
        
        // Collect all estimates for normalization
        var allEstimates = context.ProcessSteps
            .SelectMany(s => s.MatchedProviders)
            .Select(p => p.Estimate)
            .ToList();

        var costRange = (
            min: allEstimates.Min(e => (double)e.Cost),
            max: allEstimates.Max(e => (double)e.Cost)
        );

        var emissionsRange = (
            min: allEstimates.Min(e => e.EmissionsKgCO2),
            max: allEstimates.Max(e => e.EmissionsKgCO2)
        );
        
        // Diagnostic output
        var uniqueProviderIds = context.ProcessSteps
            .SelectMany(s => s.MatchedProviders.Select(p => p.ProviderId))
            .Distinct()
            .Count();

        for (int i = 0; i < context.ProcessSteps.Count; i++)
        {
            var step = context.ProcessSteps[i];
            var providerNames = string.Join(", ", step.MatchedProviders.Select(p => 
                $"{p.ProviderName}(€{p.Estimate.Cost:F0},Q{p.Estimate.QualityScore:F2})"));
        }

        // Calculate time normalization range (max possible end time from all providers)
        var windowDurationHours = (context.Request.Constraints.TimeWindow.EndTime - context.Request.Constraints.TimeWindow.StartTime).TotalHours;

        // Add cost, quality, and emissions to objective
        for (int i = 0; i < context.ProcessSteps.Count; i++)
        {
            var step = context.ProcessSteps[i];
            
            for (int j = 0; j < step.MatchedProviders.Count; j++)
            {
                var provider = step.MatchedProviders[j];
                var estimate = provider.Estimate;
                
                // Normalize metrics to 0-1 range
                var normCost = Normalize((double)estimate.Cost, costRange.min, costRange.max);
                var normQuality = estimate.QualityScore; // already 0..1
                var normEmissions = Normalize(estimate.EmissionsKgCO2, emissionsRange.min, emissionsRange.max);
                
                // Calculate coefficient (quality is inverted - higher is better)
                var coefficient = 
                    weights.CostWeight * normCost +
                    weights.EmissionsWeight * normEmissions -
                    weights.QualityWeight * normQuality;
                
                // Apply to all slots of this provider
                for (int k = 0; k < provider.IndexedSlots.Count; k++)
                {
                    objective.SetCoefficient(variables.Assignments[(i, j, k)], coefficient);
                }
            }
        }
        
        // Add time weight: normalize end time to 0-1 range using window duration
        var lastStep = context.ProcessSteps.Count - 1;
        var normalizedTimeWeight = weights.TimeWeight / windowDurationHours;
        objective.SetCoefficient(variables.EndTimes[lastStep], normalizedTimeWeight);
        
        return objective;
    }
    
    /// <summary>
    /// Extracts MIP solution with full schedule information.
    /// </summary>
    private OptimizationResult ExtractMipResult(WorkflowContext context, MipVariables variables, Objective objective, Solver.ResultStatus status)
    {
        var selectedProviders = new Dictionary<int, MatchedProvider>();
        var scheduledProcesses = new List<ScheduledProcess>();
        
        decimal totalCost = 0;
        double totalQuality = 0;
        double totalEmissions = 0;
        
        var referenceTime = context.Request.Constraints.TimeWindow.StartTime;
        
        for (int i = 0; i < context.ProcessSteps.Count; i++)
        {
            var step = context.ProcessSteps[i];
            var startHours = variables.StartTimes[i].SolutionValue();
            var endHours = variables.EndTimes[i].SolutionValue();
            
            // Find selected provider and collect ALL selected slots
            MatchedProvider? selectedProvider = null;
            var selectedSlots = new List<IndexedTimeSlot>();
            
            for (int j = 0; j < step.MatchedProviders.Count; j++)
            {
                var provider = step.MatchedProviders[j];
                
                for (int k = 0; k < provider.IndexedSlots.Count; k++)
                {
                    if (variables.Assignments[(i, j, k)].SolutionValue() > 0.5)
                    {
                        if (selectedProvider == null)
                            selectedProvider = provider;
                        
                        selectedSlots.Add(provider.IndexedSlots[k]);
                    }
                }
            }
            
            if (selectedProvider == null)
                throw new OptimizationException($"Optimization error: no provider selected for process step {i}");
            
            if (selectedSlots.Count == 0)
                throw new OptimizationException($"Optimization error: no time slot selected for process step {i}");
            
            if (selectedSlots.Count > 1)
                throw new OptimizationException($"Optimization error: invalid slot selection for process step {i}");
            
            selectedProviders[step.StepNumber] = selectedProvider;
            
            // Use the single selected slot
            var selectedSlot = selectedSlots[0];
            var allocatedSchedule = new ProviderScheduleModel
            {
                Segments = selectedSlot.Slot.Segments.Select(s => new ProviderScheduleSegmentModel
                {
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    SegmentType = s.SegmentType
                }).ToList()
            };
            
            scheduledProcesses.Add(new ScheduledProcess
            {
                StepNumber = step.StepNumber,
                Process = step.Process,
                ProviderId = selectedProvider.ProviderId,
                ProviderName = selectedProvider.ProviderName,
                AllocatedSchedule = allocatedSchedule
            });
            
            totalCost += selectedProvider.Estimate.Cost;
            totalQuality += selectedProvider.Estimate.QualityScore;
            totalEmissions += selectedProvider.Estimate.EmissionsKgCO2;
        }
        
        var firstSlot = scheduledProcesses.First().AllocatedSchedule!;
        var lastSlot = scheduledProcesses.Last().AllocatedSchedule!;
        var totalDuration = lastSlot.EndWorkingTime - firstSlot.StartWorkingTime;
       
        return new OptimizationResult
        {
            Metrics = new OptimizationMetricsModel
            {
                TotalCost = totalCost,
                TotalDuration = totalDuration,
                AverageQuality = totalQuality / context.ProcessSteps.Count,
                TotalEmissionsKgCO2 = totalEmissions,
                SolverStatus = status.ToString(),
                ObjectiveValue = objective.Value()
            },
            SelectedProviders = selectedProviders,
            Timeline = new ScheduleTimeline
            {
                ReferenceTime = referenceTime,
                Processes = scheduledProcesses
            }
        };
    }

    #endregion
}
