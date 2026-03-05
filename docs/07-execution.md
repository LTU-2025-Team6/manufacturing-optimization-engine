# Execution Lifecycle

## Overview

Once an optimization plan has strategies generated and a strategy has been selected by the user, the plan is in `Ready` status. At this point the user can confirm the strategy — this commits the plan, books the allocated time slots with all providers, and the system transitions into the execution phase.

---

## Confirming a strategy

`POST /api/strategies/{strategyId}/confirm`

This is the transition from planning to execution. `ConfirmStrategy` in `OptimizationStrategyService` does the following:

1. Validates plan is in `Ready` status (not already `Confirmed`).
2. For every process step in the selected strategy, sends `ConfirmProcessProposalCommand` to the assigned provider and waits for `ProcessProposalConfirmedEvent` — all steps in parallel via `Task.WhenAll`:

```csharp
_asyncAwaiter.AwaitAsync(new AwaitScenario<ProcessProposalConfirmedEvent>
{
    Exchange = Exchanges.Process,
    RoutingKey = $"{ProcessRoutingKeys.Confirmed}.{step.SelectedProviderId}",
    Timeout = TimeSpan.FromSeconds(10),
    Match = evt => evt.ProposalId == step.ProposalId,
    BeforeAwait = () => _messagePublisher.Publish(
        Exchanges.Process,
        $"{ProcessRoutingKeys.Confirm}.{step.SelectedProviderId}",
        new ConfirmProcessProposalCommand
        {
            ProposalId = step.ProposalId,
            SelectedSchedule = step.AllocatedSchedule   // the concrete time segments
        })
})
```

3. If any provider declines or times out, confirmation fails and the error is returned to the caller. The plan remains in `Ready` status.
4. If all providers accept, the plan status is set to `Confirmed` and an `OptimizationPlanConfirmed` notification is written for the UI to pick up on next poll.

**On the provider side**, each `ConfirmProcessProposalCommand` creates an `Execution` record in the provider's local database:
- Status: `Pending`
- `ScheduleSegments`: the exact `AllocatedSchedule` segments from the confirmed strategy step
- Linked to the proposal via `ProposalId`

From this point the time slots are booked. The `ExecutionSchedulerService` will pick them up automatically.

---

## ExecutionSchedulerService — automatic execution driving

Each provider container runs [`ExecutionSchedulerService`](../ManufacturingOptimization.ProviderSimulator/Services/ExecutionSchedulerService.cs) as a `BackgroundService`. It polls every **5 seconds** in real time and checks two groups of executions for this provider:

### Pending executions → start or fail

For each `Pending` execution:

- Get the earliest segment start time (`firstStart`) from the execution's schedule segments.
- If `now < firstStart`: too early, skip.
- If `now >= firstStart` and `now <= firstStart + dynamicTolerance`: **start** the execution.
  - Set status to `InProgress`, record `StartedAt`.
  - Publish `ProcessExecutionStartedEvent` (exchange: `process`, routing key: `process.execution-started`).
  - Write an execution notification.
- If `now > firstStart + dynamicTolerance`: **fail** as overdue.
  - Set status to `Failed`, record `CompletedAt`.
  - Publish `ProcessExecutionCompletedEvent` with `Success = false` and a reason message.

### In-progress executions → complete or fail

For each `InProgress` execution:

- Get the latest segment end time (`lastEnd`).
- If `now < lastEnd`: still running, skip.
- If `now >= lastEnd` and `now <= lastEnd + dynamicTolerance`: **complete** successfully.
  - Set status to `Completed`, record `CompletedAt`.
  - Publish `ProcessExecutionCompletedEvent` with `Success = true`.
- If `now > lastEnd + dynamicTolerance`: **fail** as overdue to complete.
  - Same as above with `Success = false`.

### Overdue tolerance

The tolerance scales with the simulation speed multiplier to avoid false failures at very high time acceleration:

```
dynamicTolerance = 60 minutes × max(1, speedMultiplier / 100)
```

At 1× speed: 60 min. At 100×: 60 min. At 1000×: 600 min (10 hours of simulated time).

---

## What Gateway does with execution events

Gateway is subscribed to `ProcessExecutionStartedEvent` and `ProcessExecutionCompletedEvent` (queue: `gateway.process.execution-events`). When these events arrive:

- `ProcessExecutionStartedEventHandler` — updates the execution step status in Gateway’s own execution tracking view, writes a notification.
- `ProcessExecutionCompletedEventHandler` — marks the step as completed or failed; if all steps in a plan are completed, the overall plan status may advance; writes a notification.

Detailed execution data (full schedule segments, exact timestamps, provider-side notes) lives only in the provider's local database. Gateway fetches it on demand via `GET /api/providers/{providerId}/executions/{executionId}`, which triggers `RequestExecutionDetailsCommand` → `ExecutionDetailsEvent` (via `AsyncAwaiter`).

---

## Monitoring execution status

`GET /api/execution/plans` — all execution plans with progress summary (paginated)  
`GET /api/execution/plans/{id}` — detailed view of a single plan's steps and their current statuses  
`GET /api/execution/plans/in-progress` — all plans currently with at least one step in progress  
`GET /api/execution/plans/{id}/steps` — list of individual step statuses for a plan  
`GET /api/execution/summary` — aggregate statistics (total plans, how many completed, in progress, failed)

---

## Extensibility note

The execution model is currently driven entirely by the simulation clock inside `ExecutionSchedulerService`. Because the entire flow is message-based (`ProcessExecutionStartedEvent`, `ProcessExecutionCompletedEvent`), a future manual mode is straightforward to add: a human operator in the field could trigger the same events explicitly — either by calling a provider API endpoint or by publishing the messages directly. The rest of the system (Gateway tracking, UI notifications, plan status updates) would work without any changes.
