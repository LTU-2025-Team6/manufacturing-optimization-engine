using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Common.Models.Exceptions;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.DTOs;
using ManufacturingOptimization.Gateway.Exceptions;

namespace ManufacturingOptimization.Gateway.Services
{
    public class OptimizationPlanService : IOptimizationPlanService
    {
        private readonly IMapper _mapper;
        private readonly IAsyncAwaiter _asyncAwaiter;
        private readonly IOptimizationPlanRepository _optimizationPlanRepository;
        private readonly IOptimizationStrategyRepository _optimizationStrategyRepository;
        private readonly IMessagePublisher _messagePublisher;
        private readonly INotificationPublisher _notificationPublisher;

        public OptimizationPlanService(
            IMapper mapper,
            IAsyncAwaiter asyncAwaiter,
            IOptimizationPlanRepository optimizationPlanRepository,
            IOptimizationStrategyRepository optimizationStrategyRepository,
            IMessagePublisher messagePublisher,
            INotificationPublisher notificationPublisher)
        {
            _mapper = mapper;
            _asyncAwaiter = asyncAwaiter;
            _optimizationPlanRepository = optimizationPlanRepository;
            _optimizationStrategyRepository = optimizationStrategyRepository;
            _messagePublisher = messagePublisher;
            _notificationPublisher = notificationPublisher;
        }

        public async Task<IEnumerable<OptimizationPlanPreviewDto>> GetAllAsync()
        {
            var plans = await _optimizationPlanRepository.GetAllAsync();

            plans.OrderByDescending(p => p.CreatedAt);

            return _mapper.Map<IEnumerable<OptimizationPlanPreviewDto>>(plans);
        }

        public async Task<OptimizationPlanDto> GetByIdAsync(Guid id)
        {
            var plan = await _optimizationPlanRepository.GetByIdAsync(id);

            if (plan == null)
                throw new NotFoundException($"Optimization plan with Id {id} not found.");

            return _mapper.Map<OptimizationPlanDto>(plan);
        }

        public async Task<CancelPlanResponse> CancelPlanAsync(Guid planId)
        {
            var plan = await _optimizationPlanRepository.GetByIdAsync(planId);
            
            if (plan == null)
                throw new NotFoundException($"Optimization plan with Id {planId} not found.");

            if (plan.Status != OptimizationPlanStatus.Confirmed.ToString())
                throw new BusinessLogicErrorException($"Only confirmed plans can be cancelled. Current status: {plan.Status}");

            if (plan.SelectedStrategyId == null)
                throw new BusinessLogicErrorException("Cannot cancel plan without selected strategy.");

            var strategy = await _optimizationStrategyRepository.GetByIdAsync(plan.SelectedStrategyId.Value);
            if (strategy == null)
                throw new NotFoundException($"Selected strategy {plan.SelectedStrategyId} not found.");

            // Cancel all confirmed proposals with providers
            var errors = new List<string>();
            var cancellationTasks = strategy.Steps.Select(step => CancelWithProviderAsync(step, errors));
            await Task.WhenAll(cancellationTasks);

            if (errors.Any())
            {
                return new CancelPlanResponse(
                    _mapper.Map<OptimizationPlanDto>(plan),
                    errors
                );
            }

            // Revert plan status back to Ready
            plan.Status = OptimizationPlanStatus.Ready.ToString();
            plan.ConfirmedAt = null;

            await _optimizationPlanRepository.UpdateAsync(plan);
            await _optimizationPlanRepository.SaveChangesAsync();

            _notificationPublisher.NotifyOptimizationPlanCancelled(plan.Id);

            return new CancelPlanResponse(_mapper.Map<OptimizationPlanDto>(plan));
        }

        public async Task DeletePlanAsync(Guid planId)
        {
            var plan = await _optimizationPlanRepository.GetByIdAsync(planId);

            if (plan == null)
                throw new NotFoundException($"Optimization plan with Id {planId} not found.");

            if (plan.Status != OptimizationPlanStatus.Ready.ToString() &&
                plan.Status != OptimizationPlanStatus.Failed.ToString())
                throw new BusinessLogicErrorException("Cannot delete plan that is not in Ready or Failed status.");

            // Break circular dependency: clear SelectedStrategyId first
            plan.SelectedStrategyId = null;
            await _optimizationPlanRepository.UpdateAsync(plan);
            await _optimizationPlanRepository.SaveChangesAsync();

            // Delete all strategies associated with this plan
            foreach (var strategy in plan.Strategies.ToList())
            {
                await _optimizationStrategyRepository.DeleteAsync(strategy);
            }
            await _optimizationStrategyRepository.SaveChangesAsync();

            // Now delete the plan itself
            await _optimizationPlanRepository.DeleteAsync(plan);
            await _optimizationPlanRepository.SaveChangesAsync();

            _notificationPublisher.NotifyOptimizationPlanDeleted(plan.Id);
        }

        private async Task CancelWithProviderAsync(ProcessStepEntity step, List<string> errors)
        {
            try
            {
                var stepModel = _mapper.Map<ProcessStepDto>(step);

                var response = await _asyncAwaiter.AwaitAsync(new AwaitScenario<ProcessCancelledEvent>
                {
                    Exchange = Exchanges.Process,
                    RoutingKey = $"{ProcessRoutingKeys.Cancelled}.{stepModel.SelectedProviderId}",
                    Timeout = TimeSpan.FromSeconds(10),
                    Match = evt => evt.ProposalId == step.ProposalId,
                    BeforeAwait = () =>
                        _messagePublisher.Publish(Exchanges.Process, $"{ProcessRoutingKeys.Cancel}.{stepModel.SelectedProviderId}", new CancelProcessCommand
                        {
                            ProposalId = step.ProposalId
                        })
                });

                if (response == null)
                    throw new OptimizationException($"Provider {stepModel.SelectedProviderName} did not respond to cancellation request within timeout. Process: {stepModel.Process}");

                if (!response.Success)
                    throw new OptimizationException($"Provider {stepModel.SelectedProviderName} failed to cancel {stepModel.Process}. Error: {response.ErrorMessage}");
            }
            catch (OptimizationException ex)
            {
                errors.Add(ex.Message);
            }
            catch (Exception ex)
            {
                var stepModel = _mapper.Map<ProcessStepDto>(step);
                errors.Add($"Provider {stepModel.SelectedProviderName} cancellation failed for {stepModel.Process}: {ex.Message}");
            }
        }
    }
}
