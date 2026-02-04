using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging;

public sealed class AsyncAwaiter : IAsyncAwaiter
{
    private readonly IMessagingInfrastructure _infrastructure;
    private readonly IMessageSubscriber _subscriber;

    public AsyncAwaiter(
        IMessagingInfrastructure infra,
        IMessageSubscriber subscriber)
    {
        _infrastructure = infra;
        _subscriber = subscriber;
    }

    public async Task<TEvent> AwaitAsync<TEvent>(AwaitScenario<TEvent> scenario) where TEvent : IMessage
    {
        var tcs = new TaskCompletionSource<TEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
        var queueName = $"await.{typeof(TEvent).Name}.{Guid.NewGuid()}";

        _infrastructure.DeclareQueue(queueName);
        _infrastructure.BindQueue(queueName, scenario.Exchange, scenario.RoutingKey);

        Action<TEvent> handler = evt =>
        {
            if (scenario.Match(evt))
                tcs.TrySetResult(evt);
        };
            
        _subscriber.Subscribe(queueName, handler);

        try
        {
            scenario.BeforeAwait?.Invoke();

            using var cts = new CancellationTokenSource(scenario.Timeout);
            var completed = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, cts.Token));

            if (completed != tcs.Task)
                throw new TimeoutException($"Timeout waiting for {typeof(TEvent).Name}");

            scenario.AfterMatch?.Invoke();
            return await tcs.Task;
        }
        finally
        {
            _subscriber.Unsubscribe(queueName);
            _infrastructure.DeleteQueue(queueName);
        }
    }
}