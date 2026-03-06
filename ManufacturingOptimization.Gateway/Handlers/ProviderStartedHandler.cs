using AutoMapper;
using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Common.Contracts;
using ManufacturingOptimization.Gateway.Data.Entities;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Settings;
using Microsoft.Extensions.Options;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;

namespace ManufacturingOptimization.Gateway.Handlers;

public class ProviderStartedHandler : IMessageHandler<ProviderStartedEvent>
{
    private readonly INotificationPublisher _notificationPublisher;

    public ProviderStartedHandler(
        INotificationPublisher notificationPublisher)
    {
        _notificationPublisher = notificationPublisher;
    }

    public Task HandleAsync(ProviderStartedEvent evt)
    {
        _notificationPublisher.NotifyProviderStarted(evt.Provider.Name);
        return Task.CompletedTask;
    }
}
