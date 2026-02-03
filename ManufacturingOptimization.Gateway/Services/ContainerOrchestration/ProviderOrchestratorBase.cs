using Docker.DotNet;
using Docker.DotNet.Models;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using Microsoft.AspNetCore.Components;

namespace ManufacturingOptimization.Gateway.Services.ContainerOrchestration;

/// <summary>
/// Base class for provider orchestrators with shared Docker functionality.
/// </summary>
public abstract class ProviderOrchestratorBase
{
    protected readonly ILogger _logger;
    protected readonly DockerClient _dockerClient;
    protected readonly IMessagingInfrastructure _messagingInfrastructure;
    protected readonly IMessageSubscriber _messageSubscriber;
    protected readonly IMessagePublisher _messagePublisher;
    protected readonly IMessageDispatcher _dispatcher;

    protected ProviderOrchestratorBase(ILogger logger)
    {
        _logger = logger;

        var dockerUri = Environment.OSVersion.Platform == PlatformID.Unix
            ? "unix:///var/run/docker.sock"
            : "npipe://./pipe/docker_engine";

        _dockerClient = new DockerClientConfiguration(new Uri(dockerUri)).CreateClient();
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
