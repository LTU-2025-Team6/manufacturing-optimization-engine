using ManufacturingOptimization.Gateway.Data.Entities;
using ManufacturingOptimization.Common.Enums;
using ManufacturingOptimization.Gateway.DTOs.Common;
using ManufacturingOptimization.Gateway.DTOs.Execution;
using ManufacturingOptimization.Gateway.Exceptions;
using ManufacturingOptimization.Gateway.Abstractions.Services;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;

namespace ManufacturingOptimization.Gateway.Services;

public class ExecutionStatusService : IExecutionStatusService
{
    private readonly IOptimizationPlanRepository _optimizationPlanRepository;

    public ExecutionStatusService(IOptimizationPlanRepository optimizationPlanRepository)
    {
        _optimizationPlanRepository = optimizationPlanRepository;
    }

    public async Task<PagedResult<ExecutionPlanSummaryDto>> GetAllPlansAsync(PaginationRequest pagination)
    {
        var skip = (pagination.PageNumber - 1) * pagination.PageSize;
        var (plans, totalCount) = await _optimizationPlanRepository.GetPagedWithSelectedStrategyStepsAsync(skip, pagination.PageSize);
        var summaries = plans.Select(MapToPlanSummary).ToList();
        return new PagedResult<ExecutionPlanSummaryDto>(summaries, pagination.PageNumber, pagination.PageSize, totalCount);
    }

    public async Task<ExecutionPlanDetailDto> GetPlanDetailAsync(Guid id)
    {
        var plan = await GetPlanWithStrategyOrThrowAsync(id);

        return new ExecutionPlanDetailDto
        {
            Id = plan.Id,
            RequestId = plan.RequestId,
            Status = plan.Status,
            CreatedAt = plan.CreatedAt,
            ConfirmedAt = plan.ConfirmedAt,
            CompletedAt = plan.CompletedAt,
            ErrorMessage = plan.ErrorMessage,
            Steps = plan.SelectedStrategy!.Steps
                .OrderBy(s => s.StepNumber)
                .Select(MapToStepDto)
                .ToList()
        };
    }

    public async Task<PagedResult<ExecutionPlanSummaryDto>> GetInProgressPlansAsync(PaginationRequest pagination)
    {
        var skip = (pagination.PageNumber - 1) * pagination.PageSize;
        var (plans, totalCount) = await _optimizationPlanRepository.GetPagedInProgressAsync(skip, pagination.PageSize);
        var inProgress = plans.Select(MapToPlanSummary).ToList();
        return new PagedResult<ExecutionPlanSummaryDto>(inProgress, pagination.PageNumber, pagination.PageSize, totalCount);
    }

    public async Task<List<ExecutionStepDto>> GetPlanStepsAsync(Guid id)
    {
        var plan = await GetPlanWithStrategyOrThrowAsync(id);

        return plan.SelectedStrategy!.Steps
            .OrderBy(s => s.StepNumber)
            .Select(MapToStepDto)
            .ToList();
    }

    public async Task<ExecutionSummaryDto> GetExecutionSummaryAsync()
    {
        var allPlans = await _optimizationPlanRepository.GetAllWithSelectedStrategyStepsAsync();
        
        var plansWithStrategy = allPlans
            .Where(p => p.SelectedStrategy != null)
            .ToList();

        var summary = new ExecutionSummaryDto
        {
            TotalPlans = plansWithStrategy.Count,
            // Plan is in progress if it has any steps with InProgress status
            InProgressPlans = plansWithStrategy.Count(p => p.SelectedStrategy!.Steps.Any(s => s.ExecutionStatus == StepExecutionStatus.InProgress)),
            CompletedPlans = plansWithStrategy.Count(p => p.Status == OptimizationPlanStatus.Completed.ToString()),
            FailedPlans = plansWithStrategy.Count(p => p.Status == OptimizationPlanStatus.Failed.ToString()),
            ConfirmedPlans = plansWithStrategy.Count(p => p.Status == OptimizationPlanStatus.Confirmed.ToString()),
            
            ActivePlans = plansWithStrategy
                .Where(p => p.SelectedStrategy!.Steps.Any(s => s.ExecutionStatus == StepExecutionStatus.InProgress))
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

    private async Task<OptimizationPlanEntity> GetPlanWithStrategyOrThrowAsync(Guid id)
    {
        var plan = await _optimizationPlanRepository.GetWithSelectedStrategyDetailsForExecutionStatusAsync(id)
            ?? throw new NotFoundException($"Plan {id} not found");

        if (plan.SelectedStrategy == null)
            throw new NotFoundException($"Plan {id} has no selected strategy");

        return plan;
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
