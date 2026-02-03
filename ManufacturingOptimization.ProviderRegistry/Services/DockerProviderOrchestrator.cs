using AutoMapper;
using Docker.DotNet.Models;
using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.ProviderRegistry.Abstractions;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace ManufacturingOptimization.ProviderRegistry.Services;

/// <summary>
/// Production mode orchestrator - creates and manages provider containers via Docker API.
/// </summary>
public class DockerProviderOrchestrator : ProviderOrchestratorBase, IProviderOrchestrator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMapper _mapper;
    private readonly IProviderValidationService _validationService;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly DockerSettings _dockerSettings;
    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly Dictionary<Guid, string> _runningProviders = []; // providerId -> containerId
    private readonly HashSet<Guid> _registeredProviders = []; // Track ProviderRegisteredEvents
    private string? _networkName;

    public DockerProviderOrchestrator(
        ILogger<DockerProviderOrchestrator> logger,
        IServiceProvider serviceProvider,
        IMapper mapper,
        IProviderValidationService validationService,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IOptions<DockerSettings> dockerSettings,
        IOptions<RabbitMqSettings> rabbitMqSettings)
        : base(logger)
    {
        _serviceProvider = serviceProvider;
        _mapper = mapper;
        _validationService = validationService;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;
        _dockerSettings = dockerSettings.Value;
        _rabbitMqSettings = rabbitMqSettings.Value;

        SetupRabbitMq();
    }

    private void SetupRabbitMq()
    {
        _messagingInfrastructure.DeclareQueue("orchestrator.provider.registered");
        _messagingInfrastructure.BindQueue("orchestrator.provider.registered", Exchanges.Provider, ProviderRoutingKeys.Registered);
        _messagingInfrastructure.PurgeQueue("orchestrator.provider.registered");
        _messageSubscriber.Subscribe<ProviderRegisteredEvent>("orchestrator.provider.registered", async evt => await HandleProviderRegistered(evt));
    }

    private async Task HandleProviderRegistered(ProviderRegisteredEvent evt)
    {
        bool allRegistered;
        int runningCount;
        int registeredCount;

        lock (_registeredProviders)
        {
            _registeredProviders.Add(evt.Provider.Id);
            registeredCount = _registeredProviders.Count;

            lock (_runningProviders)
            {
                runningCount = _runningProviders.Count;
                allRegistered = _registeredProviders.IsSupersetOf(_runningProviders.Keys);
            }
        }

        if (allRegistered)
        {
            // Wait a moment to ensure all registrations are processed
            await Task.Delay(2000);
            _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.AllRegistered, new AllProvidersRegisteredEvent());
        }
    }

    public async Task StartAllAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IProviderRepository>();
        
        var providers = await repository.GetAllAsync(cancellationToken);
        var enabledProviders = providers.Where(p => p.Enabled).ToList();

        if (enabledProviders.Count == 0)
        {
            return;
        }

        foreach (var provider in enabledProviders)
        {
            try
            {
                var mappedProvider = _mapper.Map<ProviderModel>(provider);
                var (isApproved, declinedReason) = await _validationService.ValidateAsync(mappedProvider, cancellationToken: cancellationToken);

                if (!isApproved)
                {
                    _logger.LogWarning("Provider {Type} ({Id}) declined during validation: {Reason}", provider.Type, provider.Id, declinedReason);
                    continue;
                }

                await StartAsync(provider, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate/start provider {Type} ({Id})", provider.Type, provider.Id);
            }
        }

        // Important: wait a moment before requesting registrations to ensure all containers are ready
        await Task.Delay(3000, cancellationToken);
        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.RequestRegistrationAll, new RequestProvidersRegistrationCommand());
    }

    public async Task StartAsync(ProviderEntity provider, CancellationToken cancellationToken = default)
    {
        if (_runningProviders.ContainsKey(provider.Id))
        {
            return;
        }

        var containerName = $"provider-{provider.Id}";
        var network = await GetNetworkAsync(cancellationToken);

        // Build environment variables
        var envVars = new List<string>
        {
            $"PROVIDER_TYPE={provider.Type}",
            $"Provider__ProviderId={provider.Id}",
            $"Provider__ProviderName={provider.Name}",
            $"Provider__AutoStart={provider.AutoStart}",
            $"Provider__IsRunning={provider.IsRunning}",
            $"RabbitMQ__Host={_rabbitMqSettings.Host}",
            $"RabbitMQ__Port={_rabbitMqSettings.Port}",
            $"RabbitMQ__Username={_rabbitMqSettings.Username}",
            $"RabbitMQ__Password={_rabbitMqSettings.Password}"
        };

        // Add process capabilities
        for (int i = 0; i < provider.ProcessCapabilities.Count; i++)
        {
            var capability = provider.ProcessCapabilities.ElementAt(i);
            envVars.Add($"Provider__ProcessCapabilities__{i}__Process={capability.Process}");
            envVars.Add($"Provider__ProcessCapabilities__{i}__CostPerHour={capability.CostPerHour}");
            envVars.Add($"Provider__ProcessCapabilities__{i}__SpeedMultiplier={capability.SpeedMultiplier}");
            envVars.Add($"Provider__ProcessCapabilities__{i}__QualityScore={capability.QualityScore}");
            envVars.Add($"Provider__ProcessCapabilities__{i}__EnergyConsumptionKwhPerHour={capability.EnergyConsumptionKwhPerHour}");
            envVars.Add($"Provider__ProcessCapabilities__{i}__CarbonIntensityKgCO2PerKwh={capability.CarbonIntensityKgCO2PerKwh}");
            envVars.Add($"Provider__ProcessCapabilities__{i}__UsesRenewableEnergy={capability.UsesRenewableEnergy}");
        }

        // Add technical capabilities
        envVars.Add($"Provider__TechnicalCapabilities__AxisHeight={provider.TechnicalCapabilities.AxisHeight}");
        envVars.Add($"Provider__TechnicalCapabilities__Power={provider.TechnicalCapabilities.Power}");
        envVars.Add($"Provider__TechnicalCapabilities__Tolerance={provider.TechnicalCapabilities.Tolerance}");

        // Add working hours
        if (provider.WorkingHours != null)
        {
            envVars.Add($"Provider__WorkingHours__Is24x7={provider.WorkingHours.Is24x7}");
            envVars.Add($"Provider__WorkingHours__WorkDayStartHour={provider.WorkingHours.WorkDayStartHour}");
            envVars.Add($"Provider__WorkingHours__WorkDayEndHour={provider.WorkingHours.WorkDayEndHour}");
            
            // Deserialize working days from JSON and serialize as comma-separated list
            if (!string.IsNullOrWhiteSpace(provider.WorkingHours.WorkingDaysJson))
            {
                var workingDays = JsonSerializer.Deserialize<HashSet<DayOfWeek>>(provider.WorkingHours.WorkingDaysJson);
                if (workingDays != null)
                {
                    var workingDaysStr = string.Join(",", workingDays.Select(d => (int)d));
                    envVars.Add($"Provider__WorkingHours__WorkingDays={workingDaysStr}");
                }
            }
            
            // Add breaks
            for (int i = 0; i < provider.WorkingHours.Breaks.Count; i++)
            {
                var breakPeriod = provider.WorkingHours.Breaks.ElementAt(i);
                envVars.Add($"Provider__WorkingHours__Breaks__{i}__StartHour={breakPeriod.StartHour}");
                envVars.Add($"Provider__WorkingHours__Breaks__{i}__StartMinute={breakPeriod.StartMinute}");
                envVars.Add($"Provider__WorkingHours__Breaks__{i}__DurationMinutes={breakPeriod.DurationMinutes}");
                envVars.Add($"Provider__WorkingHours__Breaks__{i}__Name={breakPeriod.Name}");
            }
        }

        var createParams = new CreateContainerParameters
        {
            Name = containerName,
            Image = _dockerSettings.ProviderImage,
            Env = envVars,
            HostConfig = new HostConfig
            {
                NetworkMode = network,
                AutoRemove = true
            },
            Labels = new Dictionary<string, string>
            {
                ["orchestration-mode"] = "production"
            }
        };

        try
        {
            var response = await _dockerClient.Containers.CreateContainerAsync(createParams, cancellationToken);
            await _dockerClient.Containers.StartContainerAsync(response.ID, new ContainerStartParameters(), cancellationToken);

            lock (_runningProviders)
            {
                _runningProviders[provider.Id] = response.ID;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start provider container: {Message}", ex.Message);
            throw;
        }
    }

    public async Task StopAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        if (!_runningProviders.TryGetValue(providerId, out var containerId))
        {
            return;
        }

        try
        {
            await _dockerClient.Containers.StopContainerAsync(
                containerId,
                new ContainerStopParameters { WaitBeforeKillSeconds = 30 },
                cancellationToken);

            _runningProviders.Remove(providerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to stop provider {providerId}");
        }
    }

    public async Task StopAllAsync(CancellationToken cancellationToken = default)
    {
        var providerIds = _runningProviders.Keys.ToList();
        foreach (var providerId in providerIds)
        {
            await StopAsync(providerId, cancellationToken);
        }
    }

    private async Task<string> GetNetworkAsync(CancellationToken cancellationToken)
    {
        if (_dockerSettings.Network != "auto")
            return _dockerSettings.Network;

        if (_networkName != null)
            return _networkName;

        try
        {
            var hostname = Environment.GetEnvironmentVariable("HOSTNAME");
            if (string.IsNullOrEmpty(hostname))
            {
                _networkName = "bridge";
                return _networkName;
            }

            var container = await _dockerClient.Containers.InspectContainerAsync(hostname, cancellationToken);
            _networkName = container.NetworkSettings.Networks.Keys.FirstOrDefault() ?? "bridge";
            
            return _networkName;
        }
        catch
        {
            _networkName = "bridge";
            return _networkName;
        }
    }
}