using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.DTOs;
using ManufacturingOptimization.Gateway.Exceptions;

namespace ManufacturingOptimization.Gateway.Services;

public class ExecutionStatusService : IExecutionStatusService
{
    private readonly IOptimizationPlanRepository _planRepo;
    private readonly ILogger<ExecutionStatusService> _logger;

    public ExecutionStatusService(
        IOptimizationPlanRepository planRepo,
        ILogger<ExecutionStatusService> logger)
    {
        _planRepo = planRepo;
        _logger = logger;
    }

    public async Task<List<ExecutionPlanSummaryDto>> GetAllPlansAsync()
    {
        var plans = await _planRepo.GetAllAsync();
        
        var summaries = plans
            .Where(p => p.SelectedStrategy != null)
            .Select(MapToPlanSummary)
            .OrderByDescending(p => p.CreatedAt)
            .ToList();

        return summaries;
    }

    public async Task<ExecutionPlanDetailDto> GetPlanDetailAsync(Guid id)
    {
        var plan = await _planRepo.GetByIdAsync(id);
        
        if (plan == null)
        {
            throw new NotFoundException($"Plan {id} not found");
        }

        if (plan.SelectedStrategy == null)
        {
            throw new NotFoundException($"Plan {id} has no selected strategy");
        }

        var detail = new ExecutionPlanDetailDto
        {
            Id = plan.Id,
            RequestId = plan.RequestId,
            Status = plan.Status,
            CreatedAt = plan.CreatedAt,
            ConfirmedAt = plan.ConfirmedAt,
            CompletedAt = plan.CompletedAt,
            ErrorMessage = plan.ErrorMessage,
            Steps = plan.SelectedStrategy.Steps
                .OrderBy(s => s.StepNumber)
                .Select(MapToStepDto)
                .ToList()
        };

        return detail;
    }

    public async Task<List<ExecutionPlanSummaryDto>> GetInProgressPlansAsync()
    {
        var plans = await _planRepo.GetAllAsync();
        
        var inProgress = plans
            .Where(p => p.SelectedStrategy != null)
            .Where(p => p.SelectedStrategy.Steps.Any(s => s.ExecutionStatus == StepExecutionStatus.InProgress))
            .Select(MapToPlanSummary)
            .OrderBy(p => p.ConfirmedAt)
            .ToList();

        return inProgress;
    }

    public async Task<List<ExecutionStepDto>> GetPlanStepsAsync(Guid id)
    {
        var plan = await _planRepo.GetByIdAsync(id);
        
        if (plan == null)
        {
            throw new NotFoundException($"Plan {id} not found");
        }

        if (plan.SelectedStrategy == null)
        {
            throw new NotFoundException($"Plan {id} has no selected strategy");
        }

        var steps = plan.SelectedStrategy.Steps
            .OrderBy(s => s.StepNumber)
            .Select(MapToStepDto)
            .ToList();

        return steps;
    }

    public async Task<ExecutionSummaryDto> GetExecutionSummaryAsync()
    {
        var allPlans = await _planRepo.GetAllAsync();
        
        var plansWithStrategy = allPlans
            .Where(p => p.SelectedStrategy != null)
            .ToList();

        var summary = new ExecutionSummaryDto
        {
            TotalPlans = plansWithStrategy.Count,
            // Plan is in progress if it has any steps with InProgress status
            InProgressPlans = plansWithStrategy.Count(p => p.SelectedStrategy.Steps.Any(s => s.ExecutionStatus == StepExecutionStatus.InProgress)),
            CompletedPlans = plansWithStrategy.Count(p => p.Status == OptimizationPlanStatus.Completed.ToString()),
            FailedPlans = plansWithStrategy.Count(p => p.Status == OptimizationPlanStatus.Failed.ToString()),
            ConfirmedPlans = plansWithStrategy.Count(p => p.Status == OptimizationPlanStatus.Confirmed.ToString()),
            
            ActivePlans = plansWithStrategy
                .Where(p => p.SelectedStrategy.Steps.Any(s => s.ExecutionStatus == StepExecutionStatus.InProgress))
                .Select(MapToPlanSummary)
                .OrderBy(p => p.ConfirmedAt)
                .ToList(),
            
            RecentlyCompleted = plansWithStrategy
                .Where(p => p.Status == OptimizationPlanStatus.Completed.ToString())
                .Select(MapToPlanSummary)
                .OrderByDescending(p => p.CompletedAt)
                .Take(10)
                .ToList(),
            
            Failed = plansWithStrategy
                .Where(p => p.Status == OptimizationPlanStatus.Failed.ToString())
                .Select(MapToPlanSummary)
                .OrderByDescending(p => p.CompletedAt)
                .Take(10)
                .ToList()
        };

        return summary;
    }

    private ExecutionPlanSummaryDto MapToPlanSummary(OptimizationPlanEntity plan)
    {
        var steps = plan.SelectedStrategy?.Steps ?? [];
        
        return new ExecutionPlanSummaryDto
        {
            Id = plan.Id,
            RequestId = plan.RequestId,
            Status = plan.Status,
            CreatedAt = plan.CreatedAt,
            ConfirmedAt = plan.ConfirmedAt,
            CompletedAt = plan.CompletedAt,
            ErrorMessage = plan.ErrorMessage,
            TotalSteps = steps.Count,
            CompletedSteps = steps.Count(s => s.ExecutionStatus == StepExecutionStatus.Completed),
            InProgressSteps = steps.Count(s => s.ExecutionStatus == StepExecutionStatus.InProgress),
            FailedSteps = steps.Count(s => s.ExecutionStatus == StepExecutionStatus.Failed),
            CancelledSteps = steps.Count(s => s.ExecutionStatus == StepExecutionStatus.Cancelled),
            PendingSteps = steps.Count(s => s.ExecutionStatus == StepExecutionStatus.Pending)
        };
    }

    private ExecutionStepDto MapToStepDto(ProcessStepEntity step)
    {
        var segments = step.ProviderSchedule?.Segments.OrderBy(s => s.StartTime).ToList();
        
        return new ExecutionStepDto
        {
            Id = step.Id,
            StepNumber = step.StepNumber,
            ProcessName = step.Process,
            ProposalId = step.ProposalId,
            ProviderId = step.SelectedProviderId,
            ProviderName = step.SelectedProviderName,
            Status = step.ExecutionStatus,
            ScheduledStart = segments?.Where(s => s.SegmentType == SegmentType.WorkingTime.ToString()).FirstOrDefault()?.StartTime,
            ScheduledEnd = segments?.Where(s => s.SegmentType == SegmentType.WorkingTime.ToString()).LastOrDefault()?.EndTime,
            EstimatedDuration = step.Estimate != null ? TimeSpan.FromHours(step.Estimate.Duration) : null,
            EstimatedCost = step.Estimate?.Cost
        };
    }
}
