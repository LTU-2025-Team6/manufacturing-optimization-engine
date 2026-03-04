using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Contracts;

namespace ManufacturingOptimization.Common.Messages;

public static class ProviderRoutingKeys
{
    public const string StartAllProviders = "provider.start-all";
    public const string AllProvidersStarted = "provider.all-started";
    public const string StopAllProviders = "provider.stop-all";
    public const string AllProvidersStopped = "provider.all-stopped";
    public const string RequestAllProviderStarted = "provider.request-all-started";
    public const string RequestProviderStarted = "provider.request-started";
    public const string StartProvider = "provider.start";
    public const string ProviderStarted = "provider.started";
    public const string StopProvider = "provider.stop";
    public const string ProviderStopped = "provider.stopped";
    public const string UpdateProvider = "provider.update";
    public const string ProviderUpdated = "provider.updated";
    public const string RequestProviderSchedule = "provider.request-schedule";
    public const string ProviderScheduleCreated = "provider.schedule-created";
    public const string RequestExecutionDetails = "provider.request-execution-details";
    public const string ExecutionDetailsProvided = "provider.execution-details-provided";
}


public class StopAllProvidersCommand : IMessage
{
}

public class AllProvidersStoppedEvent : IMessage
{
    public IReadOnlyList<Guid> ProviderIds { get; set; } = null!;
}

public class StartAllProvidersCommand : IMessage
{
}

public class AllProvidersStartedEvent : IMessage
{
}

public class StartProviderCommand : IMessage
{
    public Guid ProviderId { get; set; }
}

public class RequestAllProviderStartedCommand : IMessage
{
}

public class RequestProviderStartedCommand : IMessage
{
    public Guid ProviderId { get; set; }
}

public class ProviderStartedEvent : IMessage
{
    public ProviderModel Provider { get; set; } = null!;
}

public class StopProviderCommand : IMessage
{
    public Guid ProviderId { get; set; }
}

public class ProviderStoppedEvent : IMessage
{
    public Guid ProviderId { get; set; }
}

public class UpdateProviderCommand : IMessage
{
    public ProviderModel Provider { get; set; } = null!;
}

public class ProviderUpdatedEvent : IMessage
{
    public ProviderModel Provider { get; set; } = null!;
}

public class RequestProviderScheduleCommand : IMessage
{
    public Guid ProviderId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}

public class ProviderScheduleCreatedEvent : IMessage
{
    public Guid ProviderId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public List<ProviderDayScheduleModel> Schedules { get; set; } = null!;
}

public class RequestExecutionDetailsCommand : IMessage
{
    public Guid ProviderId { get; set; }
    public Guid ExecutionId { get; set; }
}

public class ExecutionDetailsProvidedEvent : IMessage
{
    public Guid ExecutionId { get; set; }
    public ExecutionDetailsModel Details { get; set; } = null!;
}