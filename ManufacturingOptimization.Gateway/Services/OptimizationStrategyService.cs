using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Common.Models.Exceptions;
using ManufacturingOptimization.Common.Models.Extensions;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.DTOs;
using ManufacturingOptimization.Gateway.Exceptions;

namespace ManufacturingOptimization.Gateway.Services
{
    public class OptimizationStrategyService : IOptimizationStrategyService
    {
        private readonly IMapper _mapper;
        private readonly IAsyncAwaiter _asyncAwaiter;
        private readonly IOptimizationRequestRepository _optimizationRequestRepository;
        private readonly IOptimizationPlanRepository _optimizationPlanRepository;
        private readonly IProviderRepository _providerRepository;
        private readonly IOptimizationStrategyRepository _optimizationStrategyRepository;
        private readonly IProviderScheduleRepository _providerScheduleRepository;
        private readonly IProcessEstimateRepository _processEstimateRepository;
        private readonly IAlternativeProvidersRepository _alternativeProvidersRepository;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IOptimizationDbContext _dbContext;

        public OptimizationStrategyService(
            IMapper mapper,
            IAsyncAwaiter asyncAwaiter,
            IOptimizationRequestRepository optimizationRequestRepository,
            IOptimizationPlanRepository optimizationPlanRepository,
            IProviderRepository providerRepository,
            IOptimizationStrategyRepository optimizationStrategyRepository,
            IProviderScheduleRepository providerScheduleRepository,
            IProcessEstimateRepository processEstimateRepository,
            IAlternativeProvidersRepository alternativeProvidersRepository,
            IMessagePublisher messagePublisher,
            IOptimizationDbContext dbContext)
        {
            _mapper = mapper;
            _asyncAwaiter = asyncAwaiter;
            _optimizationRequestRepository = optimizationRequestRepository;
            _optimizationPlanRepository = optimizationPlanRepository;
            _providerRepository = providerRepository;
            _optimizationStrategyRepository = optimizationStrategyRepository;
            _providerScheduleRepository = providerScheduleRepository;
            _processEstimateRepository = processEstimateRepository;
            _alternativeProvidersRepository = alternativeProvidersRepository;
            _messagePublisher = messagePublisher;
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<AlternativeProviderDto>> GetAlternativeProviders(Guid strategyId, Guid stepId, DateTime requestedStartTime, DateTime requestedEndTime)
        {
            var strategy = await _optimizationStrategyRepository.GetByIdAsync(strategyId)
                ?? throw new NotFoundException($"Optimization strategy with ID {strategyId} not found.");

            var step = strategy.Steps.FirstOrDefault(s => s.Id == stepId)
                ?? throw new NotFoundException($"Process step with ID {stepId} not found in strategy {strategyId}.");

            if (strategy.PlanId == null)
                throw new NotFoundException($"Optimization plan for strategy {strategyId} not found.");

            var plan = await _optimizationPlanRepository.GetByIdAsync((Guid)strategy.PlanId)
                ?? throw new NotFoundException($"Optimization plan with ID {strategy.PlanId} not found.");

            var request = await _optimizationRequestRepository.GetByIdAsync(plan.RequestId)
                ?? throw new NotFoundException($"Optimization request with ID {plan.RequestId} not found.");

            var capableProviders = await _providerRepository.GetProvidersWithCapabilityAsync(Enum.Parse<ProcessType>(step.Process));
            var estimationTasks = capableProviders.Select(provider => TriggerAndAwaitEstimationAsync(
                    provider.Id,
                    plan.Id,
                    Enum.Parse<ProcessType>(step.Process),
                    _mapper.Map<MotorSpecificationsModel>(request.MotorSpecs),
                    requestedStartTime,
                    requestedEndTime));

            // Trigger estimation for each capable provider and await results
            var acceptedEstimations = (await Task.WhenAll(estimationTasks))
                .Where(e => e.Accepted);

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

        public async Task<ValidateProcessTimeResponse> ValidateAlternativeProcessTime(Guid strategyId, Guid stepId, ValidateProcessTimeRequest request)
        {
            var alternatives = await _alternativeProvidersRepository.GetAlternativesForStepAsync(stepId)
                ?? throw new NotFoundException($"No alternative providers found for step {stepId}.");

            var alternative = alternatives.FirstOrDefault(a => a.ProviderId == request.ProviderId)
                ?? throw new NotFoundException($"Alternative provider with ID {request.ProviderId} not found for step {stepId}.");
            
            try
            {
                var slot = alternative.Schedule.Segments.TryBuildWorkSlot(request.RequestedStartTime, alternative.Estimate.Duration);

                if (slot == null)
                    throw new Exception("Unable to build work slot with the requested time window.");

                return new ValidateProcessTimeResponse(
                    true,
                    _mapper.Map<ProviderScheduleDto>(new ProviderScheduleModel
                    {
                        Segments = slot
                    })
                );
            }
            catch (Exception)
            {
                return new ValidateProcessTimeResponse(
                    false,
                    _mapper.Map<ProviderScheduleDto>(alternative.Schedule),
                    new[] { "The requested time window conflicts with the provider's schedule." }
                );
            }
        }

        public async Task<UpdateStrategyResponse> UpdateStrategy(Guid strategyId, UpdateStrategyRequest request)
        {
            var strategy = await _optimizationStrategyRepository.GetByIdAsync(strategyId)
                ?? throw new NotFoundException($"Optimization strategy with ID {strategyId} not found.");

            if (strategy.PlanId == null)
                throw new NotFoundException($"Optimization plan for strategy {strategyId} not found.");

            var plan = await _optimizationPlanRepository.GetByIdAsync((Guid)strategy.PlanId)
                ?? throw new NotFoundException($"Optimization plan with ID {strategy.PlanId} not found.");

            if (plan.Status == OptimizationPlanStatus.Confirmed.ToString())
                throw new BusinessLogicErrorException("Cannot update a confirmed strategy. The strategy has been finalized and locked.");

            foreach (var update in request.Updates)
            {
                var step = strategy.Steps.FirstOrDefault(s => s.Id == update.StepId)
                    ?? throw new NotFoundException($"Optimization step with ID {update.StepId} not found.");

                var providerChanged = update.NewProviderId != null && update.NewProviderId != step.SelectedProviderId;
                var timeChanged = update.NewStartTime != null && update.NewEndTime != null;

                if (providerChanged)
                    step.SelectedProviderId = (Guid)update.NewProviderId!;

                if (timeChanged)
                {
                    var alternativeProvider = await _alternativeProvidersRepository.GetOneForStepAsync(step.Id, step.SelectedProviderId)
                        ?? throw new NotFoundException($"Alternative schedule for step {step.Id} and provider {step.SelectedProviderId} not found");

                    var duration = ((DateTime)update.NewEndTime! - (DateTime)update.NewStartTime!).TotalHours;
                    var schedule = alternativeProvider.Schedule.Segments.TryBuildWorkSlot((DateTime)update.NewStartTime!, duration);

                    if (schedule == null)
                        throw new BusinessLogicErrorException("Can not build new schedule");

                    step.ProviderScheduleId = null;
                    step.ProviderSchedule = null;

                    step.ProviderSchedule = _mapper.Map<ProviderScheduleEntity>(new ProviderScheduleModel
                    {
                        Segments = schedule
                    });
                }

                // Update estimate if provider or time changed - use existing estimate from alternative provider
                if (providerChanged || timeChanged)
                {
                    var alternativeProvider = await _alternativeProvidersRepository.GetOneForStepAsync(step.Id, step.SelectedProviderId)
                        ?? throw new NotFoundException($"Alternative schedule for step {step.Id} and provider {step.SelectedProviderId} not found");

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
                    RecalculateStrategyMetrics(strategy);
                }
            }

            await _optimizationStrategyRepository.UpdateAsync(strategy);
            await _optimizationStrategyRepository.SaveChangesAsync();

            var strategyDto = _mapper.Map<OptimizationStrategyDto>(strategy);
            return new UpdateStrategyResponse(strategyDto);
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
                    _mapper.Map<OptimizationPlanDto>(plan),
                    errors
                );
            }

            plan.Status = OptimizationPlanStatus.Confirmed.ToString();
            plan.ConfirmedAt = DateTime.UtcNow;
            
            await _optimizationPlanRepository.UpdateAsync(plan);
            await _optimizationPlanRepository.SaveChangesAsync();

            return new ConfirmStrategyResponse(_mapper.Map<OptimizationPlanDto>(plan));
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
            try
            {
                var stepModel = _mapper.Map<ProcessStepModel>(step);
                
                if (stepModel.AllocatedSchedule == null)
                    throw new OptimizationException($"No allocated slot found for step {stepModel.Process} with provider {stepModel.SelectedProviderName}.");

                var response = await _asyncAwaiter.AwaitAsync(new AwaitScenario<ProcessProposalReviewedEvent>
                {
                    Exchange = Exchanges.Process,
                    RoutingKey = $"{ProcessRoutingKeys.Reviewed}.{stepModel.SelectedProviderId}",
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
                var stepModel = _mapper.Map<ProcessStepModel>(step);
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
            else
            {
                // Метрики обновляются на месте, старые значения перезаписываются
                // Удаление не требуется, так как это та же сущность
            }

            strategy.Metrics.TotalCost = totalCost;
            strategy.Metrics.TotalTime = totalDuration.Ticks;
            strategy.Metrics.AverageQuality = averageQuality;
            strategy.Metrics.TotalEmissionsKgCO2 = totalEmissions;
        }

        private async Task<OptimizationStrategyModel> EnsureStrategyExists(Guid strategyId)
        {
            var strategyEntity = await _optimizationStrategyRepository.GetByIdAsync(strategyId)
                ?? throw new NotFoundException($"Optimization strategy with ID {strategyId} not found.");

            return _mapper.Map<OptimizationStrategyModel>(strategyEntity);
        }

        private async Task<OptimizationPlanEntity> EnsurePlanExists(OptimizationStrategyModel strategy)
        {
            if (strategy.PlanId == null)
                throw new NotFoundException($"Optimization plan for strategy {strategy.Id} not found.");

            var plan = await _optimizationPlanRepository.GetByIdAsync((Guid)strategy.PlanId)
                ?? throw new NotFoundException($"Optimization plan with ID {strategy.PlanId} not found.");

            return plan;
        }
    }
}
