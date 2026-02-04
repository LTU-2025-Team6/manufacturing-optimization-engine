using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.DTOs;
using ManufacturingOptimization.Gateway.Exceptions;
using ManufacturingOptimization.Gateway.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ManufacturingOptimization.Gateway.Services
{
    public class ProviderService : IProviderService
    {
        private readonly IMapper _mapper;
        private readonly OrchestrationSettings _orchestrationSettings;
        private readonly IAsyncAwaiter _asyncAwaiter;
        private readonly IProviderRepository _providerRepository;
        private readonly IMessagePublisher _messagePublisher;

        public ProviderService(
            IMapper mapper,
            IOptions<OrchestrationSettings> orchestrationSettings,
            IAsyncAwaiter asyncAwaiter,
            IProviderRepository providerRepository,
            IMessagePublisher messagePublisher)
        {
            _mapper = mapper;
            _orchestrationSettings = orchestrationSettings.Value;
            _asyncAwaiter = asyncAwaiter;
            _providerRepository = providerRepository;
            _messagePublisher = messagePublisher;
        }

        public async Task<List<ProviderPreviewDto>> GetProvidersAsync()
        {
            var providers = await _providerRepository.GetAllAsync();
            return _mapper.Map<List<ProviderPreviewDto>>(providers);
        }

        public async Task<ProviderDto> GetProviderByIdAsync(Guid id)
        {
            var provider = await _providerRepository.GetByIdAsync(id);

            if (provider == null)
                throw new NotFoundException($"Provider with Id {id} not found.");

            return _mapper.Map<ProviderDto>(provider);
        }

        public async Task<ProviderDto> UpdateProviderAsync(Guid id, UpdateProviderRequest request)
        {
            var provider = await _providerRepository.GetByIdAsync(id);

            if (provider == null)
                throw new NotFoundException($"Provider with Id {id} not found.");

            // Update provider fields
            provider.Name = request.Name;
            provider.AutoStart = request.AutoStart;
            UpdateProcessCapabilities(provider, request.ProcessCapabilities);
            UpdateTechnicalCapabilities(provider, request.TechnicalCapabilities);
            UpdateWorkingHours(provider, request.WorkingHours);

            // Send update to provider simulator and await confirmation
            await UpdateProvider(provider);

            // Save changes to repository
            await _providerRepository.UpdateAsync(provider);
            await _providerRepository.SaveChangesAsync();

            return _mapper.Map<ProviderDto>(provider);
        }

        private void UpdateProcessCapabilities(ProviderEntity provider, List<UpdateProcessCapabilityRequest> requestCapabilities)
        {
            // Remove capabilities not in request
            var capabilitiesToRemove = provider.ProcessCapabilities
                .Where(existing => !requestCapabilities.Any(req => req.Process == existing.Process))
                .ToList();

            foreach (var capability in capabilitiesToRemove)
            {
                provider.ProcessCapabilities.Remove(capability);
            }

            // Update existing or add new capabilities
            foreach (var requestCapability in requestCapabilities)
            {
                var existingCapability = provider.ProcessCapabilities
                    .FirstOrDefault(c => c.Process == requestCapability.Process);

                if (existingCapability != null)
                {
                    // Update existing
                    existingCapability.CostPerHour = requestCapability.CostPerHour;
                    existingCapability.SpeedMultiplier = requestCapability.SpeedMultiplier;
                    existingCapability.QualityScore = requestCapability.QualityScore;
                    existingCapability.EnergyConsumptionKwhPerHour = requestCapability.EnergyConsumptionKwhPerHour;
                    existingCapability.CarbonIntensityKgCO2PerKwh = requestCapability.CarbonIntensityKgCO2PerKwh;
                    existingCapability.UsesRenewableEnergy = requestCapability.UsesRenewableEnergy;
                }
                else
                {
                    // Add new
                    provider.ProcessCapabilities.Add(new ProcessCapabilityEntity
                    {
                        Process = requestCapability.Process,
                        CostPerHour = requestCapability.CostPerHour,
                        SpeedMultiplier = requestCapability.SpeedMultiplier,
                        QualityScore = requestCapability.QualityScore,
                        EnergyConsumptionKwhPerHour = requestCapability.EnergyConsumptionKwhPerHour,
                        CarbonIntensityKgCO2PerKwh = requestCapability.CarbonIntensityKgCO2PerKwh,
                        UsesRenewableEnergy = requestCapability.UsesRenewableEnergy,
                        ProviderId = provider.Id
                    });
                }
            }
        }

        private void UpdateTechnicalCapabilities(ProviderEntity provider, UpdateTechnicalCapabilitiesRequest request)
        {
            if (provider.TechnicalCapabilities == null)
            {
                provider.TechnicalCapabilities = new TechnicalCapabilitiesEntity
                {
                    ProviderId = provider.Id
                };
            }

            provider.TechnicalCapabilities.AxisHeight = request.AxisHeight;
            provider.TechnicalCapabilities.Power = request.Power;
            provider.TechnicalCapabilities.Tolerance = request.Tolerance;
        }

        private void UpdateWorkingHours(ProviderEntity provider, UpdateWorkingHoursRequest request)
        {
            if (provider.WorkingHours == null)
            {
                provider.WorkingHours = new ProviderWorkingHoursEntity
                {
                    ProviderId = provider.Id,
                    Breaks = new List<ProviderBreakPeriodEntity>()
                };
            }

            // Update simple fields
            provider.WorkingHours.Is24x7 = request.Is24x7;
            provider.WorkingHours.WorkDayStartHour = request.WorkDayStartHour;
            provider.WorkingHours.WorkDayEndHour = request.WorkDayEndHour;

            // Serialize working days
            var workingDays = request.WorkingDays.Select(d => Enum.Parse<DayOfWeek>(d)).ToHashSet();
            provider.WorkingHours.WorkingDaysJson = JsonSerializer.Serialize(workingDays);

            // Update breaks collection
            UpdateBreakPeriods(provider.WorkingHours, request.Breaks);
        }

        private void UpdateBreakPeriods(ProviderWorkingHoursEntity workingHours, List<UpdateBreakPeriodRequest> requestBreaks)
        {
            // Clear existing breaks and add new ones (simpler than comparing)
            workingHours.Breaks.Clear();

            foreach (var requestBreak in requestBreaks)
            {
                workingHours.Breaks.Add(new ProviderBreakPeriodEntity
                {
                    StartHour = requestBreak.StartHour,
                    StartMinute = requestBreak.StartMinute,
                    DurationMinutes = requestBreak.DurationMinutes,
                    Name = requestBreak.Name,
                    ProviderId = workingHours.ProviderId
                });
            }
        }

        public async Task<ProviderPreviewDto> ToggleProviderAsync(Guid id, bool isRunning)
        {
            if (_orchestrationSettings.IsDevelopmentMode)
                throw new BusinessLogicErrorException("Toggling providers is not allowed in development mode.");

            var provider = await _providerRepository.GetByIdAsync(id);

            if (provider == null)
                throw new NotFoundException($"Provider with Id {id} not found.");                

            if (!isRunning)
                await StopProviderAsync(id);
            else
                await StartProviderAsync(id);

            provider.IsRunning = isRunning;
            return _mapper.Map<ProviderPreviewDto>(provider);
        }

        private async Task StopProviderAsync(Guid id)
        {
            await _asyncAwaiter.AwaitAsync(new AwaitScenario<ProviderStoppedEvent>
            {
                Exchange = Exchanges.Provider,
                RoutingKey = ProviderRoutingKeys.ProviderStopped,
                Timeout = TimeSpan.FromSeconds(10),
                Match = evt => evt.ProviderId == id,
                BeforeAwait = () =>
                    _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.StopProvider, new StopProviderCommand
                    {
                        ProviderId = id
                    })
            });
        }


        private async Task StartProviderAsync(Guid id)
        {
            await _asyncAwaiter.AwaitAsync(new AwaitScenario<ProviderStartedEvent>
            {
                Exchange = Exchanges.Provider,
                RoutingKey = ProviderRoutingKeys.ProviderStarted,
                Timeout = TimeSpan.FromSeconds(10),
                Match = evt => evt.Provider?.Id == id,
                BeforeAwait = () =>
                    _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.StartProvider, new StartProviderCommand
                    {
                        ProviderId = id
                    })
            });
        }

        private async Task<ProviderModel> UpdateProvider(ProviderEntity provider)
        {
            var updateResult = await _asyncAwaiter.AwaitAsync(new AwaitScenario<ProviderUpdatedEvent>
            {
                Exchange = Exchanges.Provider,
                RoutingKey = ProviderRoutingKeys.ProviderUpdated,
                Timeout = TimeSpan.FromSeconds(10),
                Match = evt => evt.Provider?.Id == provider.Id,
                BeforeAwait = () =>
                    _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.UpdateProvider, new UpdateProviderCommand
                    {
                        Provider = _mapper.Map<ProviderModel>(provider)
                    })
            });

            return updateResult.Provider;
        }
    }
}
