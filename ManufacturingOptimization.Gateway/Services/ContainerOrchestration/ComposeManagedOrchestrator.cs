using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Gateway.Abstractions;

namespace ManufacturingOptimization.Gateway.Services.ContainerOrchestration;

/// <summary>
/// Development mode orchestrator - providers managed by docker-compose.
/// Tracks ProviderRegisteredEvent and publishes AllProvidersReadyEvent when all 3 providers register.
/// </summary>
public class ComposeManagedOrchestrator : ProviderOrchestratorBase, IProviderOrchestrator
{
    public ComposeManagedOrchestrator(ILogger<ComposeManagedOrchestrator> logger) : base(logger)
    {
    }

    public Task StartAllAsync(CancellationToken cancellationToken = default)
    {
        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.RequestRegistrationAll, new RequestProvidersRegistrationCommand());
        return Task.CompletedTask;
    }
    public Task StopAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task StartAsync(ProviderEntity provider, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}