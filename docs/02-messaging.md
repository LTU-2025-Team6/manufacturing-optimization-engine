# Messaging

## Overview

All inter-service communication goes through RabbitMQ using the **Topic exchange** model. Each service declares its own queues at startup, binds them to the relevant exchanges with routing keys, and subscribes handlers for the message types it cares about. There is no central message bus or shared subscription registry — each service manages its own subscriptions independently.

The system uses four exchanges:

| Exchange | Purpose |
|---|---|
| `optimization` | Optimization requests and plan updates between Gateway and Engine |
| `process` | Process proposals, confirmations, cancellations, and execution events between Gateway and providers |
| `provider` | Provider lifecycle: start, stop, update, schedule requests |
| `system` | System readiness and simulation time changes, broadcast to all services |

---

## Message Contracts

All messages are defined in `ManufacturingOptimization.Common/Messages/` and implement the `IMessage` marker interface. There are four files, each containing both the routing key constants and the message classes for that domain:

- **`ProcessMessages.cs`** — `ProposeProcessToProviderCommand`, `ProcessProposalEstimatedEvent`, `ConfirmProcessProposalCommand`, `ProcessProposalConfirmedEvent`, `CancelProcessCommand`, and execution events (`ProcessExecutionStartedEvent`, `ProcessExecutionCompletedEvent`). Routing keys are in `ProcessRoutingKeys`.

- **`ProviderMessages.cs`** — provider lifecycle commands and events: start/stop/update individual providers or all of them, request provider schedule, request execution details. Routing keys are in `ProviderRoutingKeys`.

- **`OptimizationMessages.cs`** — `RequestOptimizationPlanCommand`, `OptimizationPlanUpdatedEvent`, `SelectStrategyCommand`. Routing keys are in `OptimizationRoutingKeys`.

- **`SystemMessages.cs`** — `ServiceReadyEvent`, `SystemReadyEvent`, `SimulationTimeChangedEvent`. Routing keys are in `SystemRoutingKeys`.

Messages are serialized as JSON by `RabbitMqService`. The message type is included as a header (`MessageType`: fully-qualified type name) to allow the dispatcher to route it to the correct handler on the receiving side.

---

## Core Interfaces

**`IMessagePublisher`** — publishes a message to an exchange with a routing key:
```
Publish<T>(string exchange, string routingKey, T message)
```

**`IMessageSubscriber`** — subscribes an action to a named queue and unsubscribes from it:
```
Subscribe<T>(string queueName, Action<T> handler)
Unsubscribe(string queueName)
```

**`IMessagingInfrastructure`** — manages queue topology: declare, bind, purge, delete queues:
```
DeclareQueue / BindQueue / PurgeQueue / DeleteQueue
```

All three interfaces are implemented by a single [`RabbitMqService`](../ManufacturingOptimization.Common.Messaging/RabbitMqService.cs) instance registered as a singleton. Each interface is mapped separately in DI so consumers only take the dependency they actually need.

---

## Message Handler Pattern

Incoming messages are dispatched through [`MessageDispatcher`](../ManufacturingOptimization.Common.Messaging/MessageDispatcher.cs), which resolves the handler for a given message type from a **new DI scope** for every message:

```csharp
using var scope = _scopeFactory.CreateScope();
var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TMessage>>();
await handler.HandleAsync(message);
```

This means each handler invocation gets a fresh scoped context — its own DbContext, repository instances, and other scoped services. Handlers are free to use EF Core, call repositories, and publish follow-up messages without any shared state between invocations.

Each message type has exactly one handler registered in DI:
```csharp
// Example from Gateway Program.cs:
services.AddScoped<IMessageHandler<ProcessExecutionStartedEvent>, ProcessExecutionStartedEventHandler>();
services.AddScoped<IMessageHandler<OptimizationPlanUpdatedEvent>, OptimizationPlanUpdatedHandler>();
services.AddScoped<IMessageHandler<ProviderStartedEvent>, ProviderStartedHandler>();
// ...
```

Subscriptions are set up in background workers ([`GatewayWorker`](../ManufacturingOptimization.Gateway/GatewayWorker.cs), [`EngineWorker`](../ManufacturingOptimization.Engine/EngineWorker.cs), [`ProviderSimulatorWorker`](../ManufacturingOptimization.ProviderSimulator/ProviderSimulatorWorker.cs)) at startup. Each worker declares the queues it needs, binds them, and wires them to `_dispatcher.DispatchAsync(message)`.

---

## AsyncAwaiter

[`AsyncAwaiter`](../ManufacturingOptimization.Common.Messaging/AsyncAwaiter.cs) (`IAsyncAwaiter`) solves the request-reply problem in an asynchronous distributed system: you need to publish a message and then wait for a specific response before continuing without polling or splitting the logic across multiple unrelated handlers.

### Why it exists

Without `AsyncAwaiter`, there are two common alternatives — both problematic:

1. **Polling** — the caller repeatedly queries a DB or external endpoint until the expected state appears. This adds latency, wastes resources, and ties up threads in a loop.

2. **Distributed handler chains** — service A publishes a command, service B handles it and publishes an event, service C handles that event and publishes the next one, each continuing the flow. In practice this scatters what is conceptually a single operation across multiple handlers in multiple files, making it hard to reason about ordering, error handling, and what happens if an intermediate step fails.

`AsyncAwaiter` lets a single service method own the entire flow: send a command, await the response as if it were a local async call, then proceed. The caller's stack is suspended but not blocked — other requests are handled normally on other threads.

In Gateway specifically this is important because it bridges the gap toward the UI: once `AwaitAsync` returns, the Gateway already has the result in-process and can write the notification record immediately — the UI picks it up on its next poll.

### How it works

`AwaitAsync<TEvent>` creates a **temporary queue** with a unique name (`await.{EventType}.{Guid}`), binds it to the specified exchange and routing key, then executes the `BeforeAwait` action (which typically publishes the outgoing command), and waits for a matching response. On completion or timeout the queue is unsubscribed and deleted.

```csharp
var result = await _asyncAwaiter.AwaitAsync(new AwaitScenario<ProviderUpdatedEvent>
{
    Exchange = Exchanges.Provider,
    RoutingKey = ProviderRoutingKeys.ProviderUpdated,
    Timeout = TimeSpan.FromSeconds(10),
    Match = evt => evt.Provider.Id == provider.Id,         // filter: only the right provider
    BeforeAwait = () => _publisher.Publish(...)            // send the command
});
```

### Internal mechanism

Internally the class uses `TaskCompletionSource<TEvent>` to bridge the RabbitMQ consumer callback (which fires on an arbitrary thread when the message arrives) into an awaitable `Task`:

```csharp
var tcs = new TaskCompletionSource<TEvent>(TaskCreationOptions.RunContinuationsAsynchronously);

Action<TEvent> handler = evt =>
{
    if (scenario.Match(evt))
        tcs.TrySetResult(evt);      // called from RabbitMQ consumer thread
};

_subscriber.Subscribe(queueName, handler);
scenario.BeforeAwait?.Invoke();     // publish the outgoing command

using var cts = new CancellationTokenSource(scenario.Timeout);
var completed = await Task.WhenAny(tcs.Task, Task.Delay(Timeout.Infinite, cts.Token));

if (completed != tcs.Task)
    throw new TimeoutException(...);
```

`Task.WhenAny` races the TCS task against a delay backed by a `CancellationTokenSource`. As soon as the matching message arrives, `TrySetResult` resolves the TCS task and `WhenAny` returns. The finally block always cleans up the temporary queue regardless of outcome.

`AwaitMultipleAsync<TEvent>` follows the same pattern but accumulates responses in a `List<TEvent>` protected by a `lock` (multiple providers can arrive concurrently on the same queue). It completes when `CompletionCondition` returns true, or on timeout returns whatever responses were collected so far.

The `Match` predicate ensures that even when multiple providers publish events on the same routing key, only the event that corresponds to the original request resolves the wait — all others are silently ignored.

### Where it's used

| Scenario | Method |
|---|---|
| Updating a provider — wait for `ProviderUpdatedEvent` before saving to DB | `AwaitAsync` |
| Starting a provider — wait for `ProviderStartedEvent` | `AwaitAsync` |
| Stopping a provider — wait for `ProviderStoppedEvent` | `AwaitAsync` |
| Requesting provider schedule — wait for `ProviderScheduleCreatedEvent` | `AwaitAsync` |
| Sending a process proposal — wait for `ProcessProposalEstimatedEvent` | `AwaitAsync` |
| Confirming a process — wait for `ProcessProposalConfirmedEvent` | `AwaitAsync` |
| Starting all providers — wait for `ProviderStartedEvent` from **all** of them | `AwaitMultipleAsync` |
| Submitting an optimization request — wait for `OptimizationPlanUpdatedEvent` from Engine | `AwaitAsync` |

---

## Provider-Specific Routing

Communication with individual providers uses routing keys that include the provider ID. This is how a message reaches exactly one provider rather than all of them.

**Targeted message** (to a specific provider):
```
simulatior.process.proposal.{providerId}    →  ProposeProcessToProviderCommand
simulatior.process.confirm.{providerId}     →  ConfirmProcessProposalCommand
simulatior.process.estimated.{providerId}   →  ProcessProposalEstimatedEvent  (response)
simulatior.process.reviewed.{providerId}    →  ProcessProposalConfirmedEvent  (response)
```

Each provider simulator creates its own queue bound to its ID-suffixed routing key at startup, so only the intended container receives the message.

**Broadcast messages** (to all providers) use routing keys without an ID:
```
provider.request-all-started   →  RequestAllProviderStartedCommand
provider.update                →  UpdateProviderCommand
system.time.changed            →  SimulationTimeChangedEvent
```

Each provider has its own queue bound to the same broadcast routing key, so all of them receive the message independently.
