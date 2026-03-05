# Provider Simulator

## What a provider is

Each provider instance is a `ManufacturingOptimization.ProviderSimulator` container. The system runs one container per provider. There is no shared code path between providers — each container is fully independent and communicates only via RabbitMQ.

A provider represents a manufacturing company capable of performing one or more process types (Cleaning, Disassembly, Redesign, Turning, etc.). Its entire identity is injected through environment variables at container startup: ID, name, what processes it can do (with cost, speed, quality, and energy parameters), technical capabilities (motor power and axis height limits), working hours per day, break schedules, and RabbitMQ connection settings. There is no config file.

---

## What provider does at runtime

After startup, a provider:

1. Reads its configuration from environment variables
2. Connects to RabbitMQ, subscribes to its own provider-specific queues (routing keys suffixed with its ID), and to broadcast queues.
3. Publishes `ProviderStartedEvent` — this is the startup confirmation that `StartAllProvidersHandler` in Gateway is waiting for.
4. Starts [`ExecutionSchedulerService`](../ManufacturingOptimization.ProviderSimulator/Services/ExecutionSchedulerService.cs) — a background worker that polls its own pending executions every 5 seconds and drives them forward based on the simulation clock.

---

## Gateway's data vs provider's data

Gateway maintains its own database (SQLite) with provider records: capabilities, technical specs, working hours, running status. This data is the source-of-truth for orchestration and optimization. It is kept synchronized through RabbitMQ events:

- When a provider is updated via `PUT /api/providers/{id}`, Gateway sends `UpdateProviderCommand` and awaits `ProviderUpdatedEvent` (via `AsyncAwaiter`) before saving to its own DB. So the provider's in-memory state and Gateway's DB are always consistent for capabilities and schedule data.
- Execution records (which process is running, when it started, when it completed, schedule segments) are stored **only in the provider's own database**. Gateway receives `ProcessExecutionStartedEvent` and `ProcessExecutionCompletedEvent` from providers and uses them to track execution progress, but detailed execution data (full schedule segments, completion time, etc.) must be fetched directly from the provider via `GET /api/providers/{providerId}/executions/{executionId}`.

---

## Provider API (via Gateway)

All provider operations are exposed through Gateway at `/api/providers`.

### Read operations

| Endpoint | Description |
|---|---|
| `GET /api/providers` | List all providers (paginated) |
| `GET /api/providers/{id}` | Get a single provider with full details |
| `GET /api/providers/{id}/schedule` | Get provider's computed schedule for a time window — this is a live request to the running provider container |
| `GET /api/providers/{providerId}/executions/{executionId}` | Get full execution details from the provider's local database |

`GET /api/providers/{id}/schedule` uses `AsyncAwaiter` internally: Gateway publishes `RequestProviderScheduleCommand` to the provider, waits for `ProviderScheduleCreatedEvent` with the generated schedule, and returns it to the caller. This is a live request — it reflects the provider's actual working-hours calendar, breaks, and any already-booked slots.

### Mutating operations

| Endpoint | Description | What happens |
|---|---|---|
| `POST /api/providers` | Create a new provider | Saved to Gateway DB only; no container started — container is started separately via PATCH |
| `PUT /api/providers/{id}` | Update provider capabilities, schedule, technical specs | `UpdateProviderCommand` → `ProviderUpdatedEvent` (AsyncAwaiter), then save to Gateway DB |
| `PATCH /api/providers/{id}` | Toggle running status (`IsRunning`) | If turning on: start container → `RequestProviderStartedCommand` → `ProviderStartedEvent`; if turning off: `RequestProviderStoppedCommand` → `ProviderStoppedEvent` → stop container |
| `DELETE /api/providers/{id}` | Delete a provider | Only if not running; removes from Gateway DB |

Updating a provider (capabilities, working hours, technical specs) propagates the changes to the running container synchronously — the request blocks until the provider acknowledges the update. This ensures Gateway's DB and the provider's runtime state are always in sync.

---

## Plan lifecycle operations

### Editing a strategy before confirmation

When the plan is in `Ready` status (strategy selected, not yet confirmed), the user can edit the selected strategy — change which provider handles a step, or adjust the scheduled time window. The editing flow always involves live communication with providers.

**Step 1 — Load alternative providers**

`POST /api/plans/{planId}/strategy/steps/{stepId}/alternatives`

For the given step, Gateway re-runs the estimation flow: it sends `ProposeProcessToProviderCommand` to all providers capable of that process type and collects `ProcessProposalEstimatedEvent` responses. The results are cached server-side per step ID and used in subsequent validate and update calls.

**Step 2 — Validate a proposed time slot**

`POST /api/plans/{planId}/strategy/steps/{stepId}/validate-slot`

Given a provider and a requested start time, checks whether the provider's schedule has a valid contiguous working-time window of the required duration (using `TryBuildWorkSlot` on the cached alternative's schedule). Returns the computed `AllocatedSchedule` segments if valid, or validation errors if not.

**Step 3 — Apply updates**

`PUT /api/plans/{planId}/strategy`

Accepts a list of step updates (each can change provider, scheduled time, or both). Gateway applies the changes to the strategy in its own database and recalculates aggregate metrics (total cost, duration, quality, emissions). No messages are sent to providers at this stage — the updated `AllocatedSchedule` will be sent during confirmation.

### Confirming a strategy

`POST /api/strategies/{strategyId}/confirm`

For every step in the selected strategy, Gateway sends `ConfirmProcessProposalCommand` to the assigned provider and waits for `ProcessProposalConfirmedEvent` (all steps in parallel). The command carries the `AllocatedSchedule` — the exact time segments the provider should block.

On the provider side, receiving this command creates an `Execution` record with status `Pending` and the given schedule segments. From this point the slots are booked and `ExecutionSchedulerService` will drive them forward automatically.

If any provider declines or times out, the confirmation returns an error and the **plan stays in `Ready`** status. No partial confirmation — all steps must succeed for the plan to become `Confirmed`.

### Cancelling a confirmed plan

`POST /api/plans/{id}/cancel`

Only `Confirmed` plans can be cancelled. For each step in the selected strategy, Gateway sends `CancelProcessCommand` to the assigned provider and waits for `ProcessCancelledEvent`. On the provider side this cancels (deletes or marks as cancelled) the `Execution` record, freeing the reserved time slots.

If all providers acknowledge the cancellation, the plan reverts to `Ready` status (`ConfirmedAt` is cleared) and a cancellation notification is written for the UI to pick up. If some providers fail to respond, the errors are returned but the plan status is not changed — the user can retry.

After cancellation the strategy can be re-edited or re-confirmed.

### Deleting a plan

`DELETE /api/plans/{id}`

Only plans in `Ready` or `Failed` status can be deleted. This removes the plan, all its strategies, and associated steps from Gateway's database.

---

## Provider responses to optimization

During the optimization pipeline, providers receive two types of proposals:

**1. Estimation proposal (`ProposeProcessToProviderCommand`)**  
The provider receives a process type, motor specs, and a requested time window. It runs `EstimationService` to determine whether it can perform this process, when it is available, and what it would cost. It responds with `ProcessProposalEstimatedEvent` carrying the estimate (cost, duration, quality score, emissions) and a proposed schedule. If it cannot accept the proposal, it sets `Accepted = false`.

**2. Confirmation (`ConfirmProcessProposalCommand`)**  
If the user confirms a strategy, the provider receives the final `AllocatedSchedule` — the specific schedule segments selected by the optimizer. It stores this as an `Execution` record in its local database with status `Pending` and responds with `ProcessProposalConfirmedEvent`. From this point the execution slot is booked and `ExecutionSchedulerService` will drive it forward automatically.

See [07-execution.md](07-execution.md) for what happens after confirmation — how `ExecutionSchedulerService` automatically drives executions forward based on the simulation clock.
