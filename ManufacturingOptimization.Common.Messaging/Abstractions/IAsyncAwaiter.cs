using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Abstractions;

public interface IAsyncAwaiter
{
    Task<TEvent> AwaitAsync<TEvent>(AwaitScenario<TEvent> scenario) where TEvent : IMessage;
    Task<IReadOnlyList<TEvent>> AwaitMultipleAsync<TEvent>(AwaitMultipleScenario<TEvent> scenario) where TEvent : IMessage;
}

public sealed class AwaitScenario<TEvent>
{
    public string Exchange { get; init; } = default!;
    public string RoutingKey { get; init; } = default!;
    public Func<TEvent, bool> Match { get; init; } = default!;
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    public Action? BeforeAwait { get; init; }
    public Action? AfterMatch { get; init; }
}

public sealed class AwaitMultipleScenario<TEvent>
{
    public string Exchange { get; init; } = default!;
    public string RoutingKey { get; init; } = default!;
    public Func<TEvent, bool> Match { get; init; } = _ => true;
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
    public Func<IReadOnlyList<TEvent>, bool> CompletionCondition { get; init; } = default!;
    public Action? BeforeAwait { get; init; }
    public Action<IReadOnlyList<TEvent>>? AfterCompletion { get; init; }
}