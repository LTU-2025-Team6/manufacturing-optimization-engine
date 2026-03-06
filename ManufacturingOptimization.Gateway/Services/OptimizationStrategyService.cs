using AutoMapper;
using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Contracts;
using ManufacturingOptimization.Common.Enums;
using ManufacturingOptimization.Common.Exceptions;
using ManufacturingOptimization.Common.Extensions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;
using ManufacturingOptimization.Gateway.Abstractions.Services;
using ManufacturingOptimization.Gateway.Data.Entities;
using ManufacturingOptimization.Gateway.DTOs.OptimizationPlan;
using ManufacturingOptimization.Gateway.DTOs.Provider;
using ManufacturingOptimization.Gateway.DTOs.Strategy;
using ManufacturingOptimization.Gateway.Exceptions;

namespace ManufacturingOptimization.Gateway.Services;

public class OptimizationStrategyService : IOptimizationStrategyService
{
        private readonly IMapper _mapper;
        private readonly IAsyncAwaiter _asyncAwaiter;
        private readonly IOptimizationRequestRepository _optimizationRequestRepository;
        private readonly IOptimizationPlanRepository _optimizationPlanRepository;
        private readonly IProviderRepository _providerRepository;
        private readonly IOptimizationStrategyRepository _optimizationStrategyRepository;
        private readonly IAlternativeProvidersRepository _alternativeProvidersRepository;
        private readonly IMessagePublisher _messagePublisher;
        private readonly INotificationPublisher _notificationPublisher;

        public OptimizationStrategyService(
            IMapper mapper,
            IAsyncAwaiter asyncAwaiter,
            IOptimizationRequestRepository optimizationRequestRepository,
            IOptimizationPlanRepository optimizationPlanRepository,
            IProviderRepository providerRepository,
            IOptimizationStrategyRepository optimizationStrategyRepository,
            IAlternativeProvidersRepository alternativeProvidersRepository,
            IMessagePublisher messagePublisher,
            INotificationPublisher notificationPublisher)
        {
            _mapper = mapper;
            _asyncAwaiter = asyncAwaiter;
            _optimizationRequestRepository = optimizationRequestRepository;
            _optimizationPlanRepository = optimizationPlanRepository;
            _providerRepository = providerRepository;
            _optimizationStrategyRepository = optimizationStrategyRepository;
            _alternativeProvidersRepository = alternativeProvidersRepository;
            _messagePublisher = messagePublisher;
            _notificationPublisher = notificationPublisher;
        }

        public async Task<IEnumerable<AlternativeProviderDto>> GetAlternativeProvidersAsync(Guid planId, Guid stepId, DateTime windowStart, DateTime windowEnd)
        {
            var plan = await _optimizationPlanRepository.GetByIdAsync(planId)
                ?? throw new NotFoundException($"Optimization plan with ID {planId} not found.");

            if (plan.SelectedStrategyId == null)
                throw new NotFoundException($"Plan {planId} has no selected strategy.");

            var strategy = await _optimizationStrategyRepository.GetByIdWithStepsOnlyAsync(plan.SelectedStrategyId.Value)
                ?? throw new NotFoundException($"Selected strategy {plan.SelectedStrategyId} not found.");

            var step = strategy.Steps.FirstOrDefault(s => s.Id == stepId)
                ?? throw new NotFoundException($"Process step with ID {stepId} not found in strategy {strategy.Id}.");

            var request = await _optimizationRequestRepository.GetByIdAsync(plan.RequestId)
                ?? throw new NotFoundException($"Optimization request with ID {plan.RequestId} not found.");

            var capableProviders = await _providerRepository.GetProvidersWithCapabilityAsync(Enum.Parse<ProcessType>(step.Process));
            var estimationTasks = capableProviders.Select(provider => TriggerAndAwaitEstimationAsync(
                provider.Id,
                plan.Id,
                Enum.Parse<ProcessType>(step.Process),
                _mapper.Map<MotorSpecificationsModel>(request.MotorSpecs),
                windowStart,
                windowEnd));

            var acceptedEstimations = (await Task.WhenAll(estimationTasks))
                .Where(e => e.Accepted)
                .ToList();

            await _alternativeProvidersRepository.AddAlternativesForStepAsync(
                stepId,
                acceptedEstimations.Select(e => new AlternativeProviderModel(
                    e.ProviderId,
                    e.ProviderName,
                    e.ProposalId ?? Guid.Empty,
                    e.Estimate!,
                    e.Schedule!
                )).ToList()
            );

            return acceptedEstimations.Select(e => new AlternativeProviderDto
            {
                ProviderId = e.ProviderId,
                ProviderName = e.ProviderName,
                Estimate = _mapper.Map<ProcessEstimateDto>(e.Estimate),
                Schedule = _mapper.Map<ProviderScheduleDto>(e.Schedule)
            });
        }

        public async Task<ValidateSlotResponse> ValidateSlotAsync(Guid planId, Guid stepId, ValidateSlotRequest request)
        {
            var alternatives = await _alternativeProvidersRepository.GetAlternativesForStepAsync(stepId)
                ?? throw new NotFoundException($"No cached alternatives found for step {stepId}. Load alternatives first.");

            var alternative = alternatives.FirstOrDefault(a => a.ProviderId == request.ProviderId)
                ?? throw new NotFoundException($"Alternative provider {request.ProviderId} not found for step {stepId}.");

            var slot = alternative.Schedule.Segments.TryBuildWorkSlot(request.RequestedStart, request.DurationHours);

            if (slot == null)
                return new ValidateSlotResponse(false, null, ["Unable to fit the requested start time within the provider's available schedule."]);

            var workSegments = slot.Where(s => s.SegmentType == SegmentType.WorkingTime).ToList();
            var allocatedSchedule = new ProviderScheduleDto
            {
                StartWorkingTime = workSegments.Count > 0 ? workSegments.Min(s => s.StartTime) : slot[0].StartTime,
                EndWorkingTime   = workSegments.Count > 0 ? workSegments.Max(s => s.EndTime)   : slot[^1].EndTime,
                Segments = _mapper.Map<List<ProviderScheduleSegmentDto>>(slot.ToList())
            };

            return new ValidateSlotResponse(true, allocatedSchedule, null);
        }

        public async Task<UpdateStrategyResponse> UpdateStrategyAsync(Guid planId, UpdateStrategyRequest request)
        {
            var plan = await _optimizationPlanRepository.GetByIdAsync(planId)
                ?? throw new NotFoundException($"Optimization plan with ID {planId} not found.");

            if (plan.SelectedStrategyId == null)
                throw new NotFoundException($"Plan {planId} has no selected strategy.");

            if (plan.Status == OptimizationPlanStatus.Confirmed.ToString())
                throw new BusinessLogicErrorException("Cannot update a confirmed strategy. The strategy has been finalized and locked.");

            var strategy = await _optimizationStrategyRepository.GetByIdAsync(plan.SelectedStrategyId.Value)
                ?? throw new NotFoundException($"Selected strategy {plan.SelectedStrategyId} not found.");

            foreach (var update in request.StepUpdates)
            {
                var step = strategy.Steps.FirstOrDefault(s => s.Id == update.StepId)
                    ?? throw new NotFoundException($"Optimization step with ID {update.StepId} not found.");

                var providerChanged = update.ProviderId.HasValue && update.ProviderId.Value != step.SelectedProviderId;
                var timeChanged = update.ScheduledStart != DateTime.MinValue && update.ScheduledEnd != DateTime.MinValue;

                if (providerChanged)
                    step.SelectedProviderId = update.ProviderId!.Value;

                if (providerChanged || timeChanged)
                {
                    var alternativeProvider = await _alternativeProvidersRepository.GetOneForStepAsync(step.Id, step.SelectedProviderId)
                        ?? throw new NotFoundException($"Alternative schedule for step {step.Id} and provider {step.SelectedProviderId} not found.");

                    if (timeChanged)
                    {
                        // Use the provider's actual working-hours duration, not the wall-clock span
                        // (wall-clock span > working hours whenever the slot spans a break)
                        var schedule = alternativeProvider.Schedule.Segments.TryBuildWorkSlot(update.ScheduledStart, alternativeProvider.Estimate.Duration);

                        if (schedule == null)
                            throw new BusinessLogicErrorException("Unable to build a valid schedule for the requested time window.");

                        step.ProviderScheduleId = null;
                        step.ProviderSchedule = _mapper.Map<ProviderScheduleEntity>(new ProviderScheduleModel
                        {
                            Segments = schedule
                        });
                    }

                    if (step.Estimate == null)
                    {
                        step.Estimate = _mapper.Map<ProcessEstimateEntity>(alternativeProvider.Estimate);
                    }
                    else
                    {
                        step.Estimate.Duration = alternativeProvider.Estimate.Duration;
                        step.Estimate.Cost = alternativeProvider.Estimate.Cost;
                        step.Estimate.QualityScore = alternativeProvider.Estimate.QualityScore;
                        step.Estimate.EmissionsKgCO2 = alternativeProvider.Estimate.EmissionsKgCO2;
                    }

                    step.ProposalId = alternativeProvider.ProposalId;

                    if (providerChanged)
                        step.SelectedProviderName = alternativeProvider.ProviderName;

                    RecalculateStrategyMetrics(strategy);
                }
            }

            await _optimizationStrategyRepository.UpdateAsync(strategy);
            await _optimizationStrategyRepository.SaveChangesAsync();

            var updatedStrategyDto = _mapper.Map<OptimizationStrategyDto>(strategy);
            return new UpdateStrategyResponse(updatedStrategyDto, null);
        }

        public async Task<ConfirmStrategyResponse> ConfirmStrategy(Guid strategyId)
        {
            var strategy = await _optimizationStrategyRepository.GetByIdAsync(strategyId)
                ?? throw new NotFoundException($"Optimization strategy with ID {strategyId} not found.");

            if (strategy.PlanId == null)
                throw new NotFoundException($"Optimization plan for strategy {strategyId} not found.");

            var plan = await _optimizationPlanRepository.GetByIdAsync((Guid)strategy.PlanId)
                ?? throw new NotFoundException($"Optimization plan with ID {strategy.PlanId} not found.");

            if (plan.Status == OptimizationPlanStatus.Confirmed.ToString())
                throw new BusinessLogicErrorException("Strategy is already confirmed.");

            if (plan.Status != OptimizationPlanStatus.Ready.ToString())
                throw new BusinessLogicErrorException($"Strategy must be in Ready status to be confirmed. Current status: {plan.Status}");

            var errors = new List<string>();
            var confirmationTasks = strategy.Steps.Select(step => ConfirmWithProviderAsync(step, errors));
            await Task.WhenAll(confirmationTasks);

            if (errors.Any())
            {
                return new ConfirmStrategyResponse(
                    false,
                    string.Join(";  ", errors)
                );
            }

            plan.Status = OptimizationPlanStatus.Confirmed.ToString();
            plan.ConfirmedAt = DateTime.UtcNow;

            _notificationPublisher.NotifyOptimizationPlanConfirmed(plan.Id);
            
            await _optimizationPlanRepository.UpdateAsync(plan);
            await _optimizationPlanRepository.SaveChangesAsync();

            return new ConfirmStrategyResponse(true, null);
        }

        private async Task<ProcessProposalEstimatedEvent> TriggerAndAwaitEstimationAsync(
            Guid providerId,
            Guid planId,
            ProcessType process,
            MotorSpecificationsModel motorSpecifications,
            DateTime scheduleStartTime,
            DateTime scheduleEndTime)
        {
            return await _asyncAwaiter.AwaitAsync(new AwaitScenario<ProcessProposalEstimatedEvent>
            {
                Exchange = Exchanges.Process,
                RoutingKey = $"{ProcessRoutingKeys.Estimated}.{providerId}",
                Timeout = TimeSpan.FromSeconds(10),
                Match = evt => true,
                BeforeAwait = () =>
                    _messagePublisher.Publish(Exchanges.Process, $"{ProcessRoutingKeys.Propose}.{providerId}", new ProposeProcessToProviderCommand
                    {
                        ProviderId = providerId,
                        Process = process,
                        MotorSpecs = motorSpecifications,
                        PlanId = planId,
                        RequestedTimeWindow = new TimeWindowModel
                        {
                            StartTime = scheduleStartTime,
                            EndTime = scheduleEndTime
                        }
                    })
            });
        }

        private async Task ConfirmWithProviderAsync(ProcessStepEntity step, List<string> errors)
        {
            var stepModel = _mapper.Map<ProcessStepModel>(step);
            try
            {
                if (stepModel.AllocatedSchedule == null)
                    throw new OptimizationException($"No allocated slot found for step {stepModel.Process} with provider {stepModel.SelectedProviderName}.");

                var response = await _asyncAwaiter.AwaitAsync(new AwaitScenario<ProcessProposalConfirmedEvent>
                {
                    Exchange = Exchanges.Process,
                    RoutingKey = $"{ProcessRoutingKeys.Confirmed}.{stepModel.SelectedProviderId}",
                    Timeout = TimeSpan.FromSeconds(10),
                    Match = evt => evt.ProposalId == stepModel.ProposalId,
                    BeforeAwait = () =>
                        _messagePublisher.Publish(Exchanges.Process, $"{ProcessRoutingKeys.Confirm}.{stepModel.SelectedProviderId}", new ConfirmProcessProposalCommand
                        {
                            ProposalId = stepModel.ProposalId,
                            SelectedSchedule = stepModel.AllocatedSchedule
                        })
                });

                if (response == null)
                    throw new OptimizationException($"Provider {stepModel.SelectedProviderName} did not respond to confirmation request within timeout. Process: {stepModel.Process}");

                if (!response.IsAccepted)
                    throw new OptimizationException($"Provider {stepModel.SelectedProviderName} declined confirmation for {stepModel.Process}. Reason: {response.DeclineReason}");
            }
            catch (OptimizationException ex)
            {
                errors.Add(ex.Message);
            }
            catch (Exception ex)
            {
                errors.Add($"Provider {stepModel.SelectedProviderName} confirmation failed for {stepModel.Process}: {ex.Message}");
            }
        }

        private void RecalculateStrategyMetrics(OptimizationStrategyEntity strategy)
        {
            var stepsWithEstimates = strategy.Steps.Where(s => s.Estimate != null).ToList();
            
            if (stepsWithEstimates.Count == 0)
                return;

            var totalCost = stepsWithEstimates.Sum(s => s.Estimate!.Cost);
            var totalDuration = TimeSpan.FromHours(stepsWithEstimates.Sum(s => s.Estimate!.Duration));
            var averageQuality = stepsWithEstimates.Average(s => s.Estimate!.QualityScore);
            var totalEmissions = stepsWithEstimates.Sum(s => s.Estimate!.EmissionsKgCO2);

            if (strategy.Metrics == null)
            {
                strategy.Metrics = new OptimizationMetricsEntity
                {
                    Id = Guid.NewGuid(),
                    StrategyId = strategy.Id
                };
            }

            strategy.Metrics.TotalCost = totalCost;
            strategy.Metrics.TotalTime = totalDuration.Ticks;
            strategy.Metrics.AverageQuality = averageQuality;
            strategy.Metrics.TotalEmissionsKgCO2 = totalEmissions;
        }
}
