using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models.Contracts;

namespace ManufacturingOptimization.Common.Messaging.Messages;

public static class ProviderRoutingKeys
{
    public const string StartAllProviders = "provider.start-all";
    public const string AllProvidersStarted = "provider.all-started";
    public const string StopAllProviders = "provider.stop-all";
    public const string AllProvidersStopped = "provider.all-stopped";
    public const string StartProvider = "provider.start";
    public const string ProviderStarted = "provider.started";
    public const string StopProvider = "provider.stop";
    public const string ProviderStopped = "provider.stopped";
    public const string UpdateProvider = "provider.update";
    public const string ProviderUpdated = "provider.updated";
}


public class StopAllProvidersCommand : BaseCommand
{
}

public class AllProvidersStoppedEvent : BaseEvent
{
    public IReadOnlyList<Guid> ProviderIds { get; set; } = null!;
}

public class StartAllProvidersCommand : BaseCommand
{
}

public class AllProvidersStartedEvent : BaseEvent
{
    public IReadOnlyList<ProviderModel> RunningProviders { get; set; } = null!;
}

public class StartProviderCommand : BaseCommand
{
    public Guid ProviderId { get; set; }
}

public class ProviderStartedEvent : BaseEvent
{
    public ProviderModel Provider { get; set; } = null!;
}

public class StopProviderCommand : BaseCommand
{
    public Guid ProviderId { get; set; }
}

public class ProviderStoppedEvent : BaseEvent
{
    public Guid ProviderId { get; set; }
}

public class UpdateProviderCommand : BaseCommand
{
    public ProviderModel Provider { get; set; } = null!;
}

public class ProviderUpdatedEvent : BaseEvent
{
    public ProviderModel Provider { get; set; } = null!;
}