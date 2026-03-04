using AutoMapper;
using Docker.DotNet;
using Docker.DotNet.Models;
using ManufacturingOptimization.Gateway.Data.Entities;
using ManufacturingOptimization.Common.Settings;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ManufacturingOptimization.Gateway.Services;

/// <summary>
/// Production mode orchestrator - creates and manages provider containers via Docker API.
/// </summary>
public class DockerProviderOrchestrator : IProviderOrchestrator
{
    private readonly ILogger<DockerProviderOrchestrator> _logger;
    private readonly DockerClient _dockerClient;
    private readonly DockerSettings _dockerSettings;
    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly Dictionary<Guid, string> _runningProviders = []; // providerId -> containerId
    private string? _networkName;

    public DockerProviderOrchestrator(
        ILogger<DockerProviderOrchestrator> logger,
        IMapper mapper,
        IOptions<DockerSettings> dockerSettings,
        IOptions<RabbitMqSettings> rabbitMqSettings)
    {
        _logger = logger;
        _dockerSettings = dockerSettings.Value;
        _rabbitMqSettings = rabbitMqSettings.Value;

        var dockerUri = Environment.OSVersion.Platform == PlatformID.Unix
            ? "unix:///var/run/docker.sock"
            : "npipe://./pipe/docker_engine";

        _dockerClient = new DockerClientConfiguration(new Uri(dockerUri)).CreateClient();
    }

    public async Task StartAsync(ProviderEntity provider, CancellationToken cancellationToken = default)
    {
        if (_runningProviders.TryGetValue(provider.Id, out var containerId))
            throw new InvalidDataException($"Provider with ID {provider.Id} is already running in container {containerId}.");

        var containerName = $"provider-{provider.Id}";
        var network = await GetNetworkAsync(cancellationToken);

        var createParams = new CreateContainerParameters
        {
            Name = containerName,
            Image = _dockerSettings.ProviderImage,
            Env = BuildEnvironmentForProvider(provider),
            HostConfig = new HostConfig
            {
                NetworkMode = network,
                AutoRemove = true,
                Binds = new List<string>
                {
                    // Mount shared volume for SQLite database - all providers use same DB
                    "provider_simulator_data:/app/Data"
                }
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
            throw new InvalidDataException($"Provider with ID {providerId} is not registered for orchestration.");

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

    private List<string> BuildEnvironmentForProvider(ProviderEntity provider)
    {
        var envVars = new List<string>
        {
            $"PROVIDER_TYPE={provider.Type}",
            $"Provider__ProviderId={provider.Id}",
            $"Provider__ProviderName={provider.Name}",
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

        return envVars;
    }

    public async Task CleanupOrphanedContainersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var allContainers = await _dockerClient.Containers.ListContainersAsync(
                new ContainersListParameters { All = true },
                cancellationToken);

            var orphanedContainers = allContainers
                .Where(c =>
                {
                    var labels = c.Labels ?? new Dictionary<string, string>();
                    return labels.TryGetValue("orchestration-mode", out var mode) && mode == "production";
                })
                .ToList();

            foreach (var container in orphanedContainers)
            {
                await _dockerClient.Containers.RemoveContainerAsync(
                    container.ID,
                    new ContainerRemoveParameters { Force = true },
                    cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup orphaned containers");
        }
    }
}