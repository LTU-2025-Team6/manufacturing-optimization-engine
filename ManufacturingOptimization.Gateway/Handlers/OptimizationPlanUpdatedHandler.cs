using AutoMapper;
using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Enums;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;
using ManufacturingOptimization.Gateway.Data.Entities;

namespace ManufacturingOptimization.Gateway.Handlers;

/// <summary>
/// Handles optimization plan status update events.
/// Creates or updates plan in database as optimization progresses.
/// </summary>
public class OptimizationPlanUpdatedHandler : IMessageHandler<OptimizationPlanUpdatedEvent>
{
    private readonly IOptimizationPlanRepository _planRepository;
    private readonly IOptimizationStrategyRepository _strategyRepository;
    private readonly IMapper _mapper;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly ILogger<OptimizationPlanUpdatedHandler> _logger;

    public OptimizationPlanUpdatedHandler(
        IOptimizationPlanRepository planRepository,
        IOptimizationStrategyRepository strategyRepository,
        IMapper mapper,
        INotificationPublisher notificationPublisher,
        ILogger<OptimizationPlanUpdatedHandler> logger)
    {
        _planRepository = planRepository;
        _strategyRepository = strategyRepository;
        _mapper = mapper;
        _notificationPublisher = notificationPublisher;
        _logger = logger;
    }

    public async Task HandleAsync(OptimizationPlanUpdatedEvent evt)
    {
        var existingPlan = await _planRepository.GetWithAllStrategiesForUpdateAsync(evt.Plan.Id);

        if (existingPlan == null)
            return;

        existingPlan.Status = evt.Plan.Status.ToString();

        switch (evt.Plan.Status)
        {
            case OptimizationPlanStatus.AwaitingStrategySelection:
                var strategyEntities = _mapper.Map<List<OptimizationStrategyEntity>>(evt.Plan.Strategies);

                await _strategyRepository.AddRangeAsync(strategyEntities);
                await _strategyRepository.SaveChangesAsync();
                break;

            case OptimizationPlanStatus.StrategySelected:
                if (evt.Plan.SelectedStrategy == null)
                    throw new InvalidOperationException("Selected strategy is null in StrategySelected status");

                if (!existingPlan.Strategies.Any(s => s.Id == evt.Plan.SelectedStrategy.Id))
                    throw new InvalidOperationException($"Selected strategy {evt.Plan.SelectedStrategy.Id} not found");

                existingPlan.SelectedStrategyId = evt.Plan.SelectedStrategy.Id;
                existingPlan.SelectedAt = evt.Plan.SelectedAt;

                // Remove declined strategies
                var declinedStranegies = existingPlan.Strategies.Where(s => s.Id != evt.Plan.SelectedStrategy.Id).ToList();
                foreach (var strategy in declinedStranegies)
                {
                    existingPlan.Strategies.Remove(strategy);
                    await _strategyRepository.DeleteAsync(strategy);
                    await _strategyRepository.SaveChangesAsync();
                }
                break;

            case OptimizationPlanStatus.Confirmed:
                existingPlan.ConfirmedAt = DateTime.UtcNow;
                break;

            case OptimizationPlanStatus.Completed:
                existingPlan.CompletedAt = evt.Plan.CompletedAt ?? DateTime.UtcNow;
                break;
        }

        await _planRepository.UpdateAsync(existingPlan);
        await _planRepository.SaveChangesAsync();
        _notificationPublisher.NotifyOptimizationPlanUpdated(existingPlan.Id, existingPlan.Status);
    }
}
