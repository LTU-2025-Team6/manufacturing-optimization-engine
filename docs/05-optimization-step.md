# OptimizationStep — Preparing the Solution Space

[`OptimizationStep`](../ManufacturingOptimization.Engine/OptimizationPipeline/OptimizationStep.cs) implements `IWorkflowStep` and is the most computationally intensive step in the pipeline. Its job is not simply to "pick the best providers" but to build a complete, structured model of all feasible execution possibilities and then find the optimal assignment across all of them simultaneously.

---

## What this step receives

By the time `OptimizationStep` runs, the `WorkflowContext` already contains:

- `ProcessSteps` — the ordered sequence of manufacturing processes (e.g. Cleaning → Disassembly → … → Certification)
- For each process step: a list of `MatchedProviders` — providers that have the required capability and meet technical requirements
- For each matched provider: an `Estimate` (cost, duration, quality score, emissions) and a `Schedule` — the provider's working calendar for the requested time window

---

## Phase 1 — Building the solution space

Before the solver is invoked, `PreprocessTimeSlots` converts each provider's schedule into a list of **concrete time slot options**.

A provider's schedule is a sequence of segments (working time, breaks, unavailability). Given the process `Duration` from the estimate and a fixed granularity of **60 minutes**, `BuildPossibleWorkSlots` generates every possible start time within the requested time window that results in a valid, contiguous working-time segment of the required length. Each such candidate is an `IndexedTimeSlot`:

```csharp
new IndexedTimeSlot
{
    SlotIndex = index,
    Slot = new ProviderScheduleModel { Segments = segments },
    StartTimeHours = (startTime - referenceTime).TotalHours,
    EndTimeHours   = (endTime   - referenceTime).TotalHours
}
```

All times are stored as **hours relative to the start of the requested time window** (the reference point). This normalisation makes the MIP formulation independent of clock values.

The result for each `(process step, provider)` pair is `provider.IndexedSlots` — a list of feasible execution windows. A provider with a busy schedule and a long process may have only a handful of slots; a provider with open availability over a multi-day window may have hundreds.

**This is the solution space**: for every process step there are multiple providers, and for every provider there are multiple time slots. The solver must choose exactly one slot (implying one provider) per step.

---

## Phase 2 — MIP formulation

The step builds a **Mixed Integer Programming (MIP)** problem using **Google OR-Tools SCIP solver**.

### Decision variables

Three-dimensional binary variable for each feasible `(step, provider, slot)` triple:

```
x[i, j, k] = 1  if provider j is assigned to step i in time slot k
x[i, j, k] = 0  otherwise
```

Continuous variables for temporal reasoning:

```
start[i]  — start time of step i (hours from reference)
end[i]    — end time of step i (hours from reference)
```

The total number of binary variables is `Σ (providers_per_step[i] × slots_per_provider[i][j])` across all steps. In a typical scenario this can be several thousand variables.

### Constraints

**One slot per step** — exactly one `(provider, slot)` combination must be selected for each process step:

```
Σ_j Σ_k x[i,j,k] = 1   for each step i
```

**Sequential execution** — a process cannot start before the previous one finishes:

```
start[i+1] >= end[i]   for i = 0 .. n-2
```

**Time slot binding** — when a slot is selected, the step's start/end variables are pinned to that slot's times. This is expressed via Big-M constraints:

```
If x[i,j,k] = 1 → start[i] ≤ slot.StartTimeHours
If x[i,j,k] = 1 → end[i]   ≥ slot.EndTimeHours
```

**Deadline** — the last step must finish within the requested time window:

```
end[last] ≤ windowDurationHours
```

**Budget** (optional) — if `MaxBudget` is set in the request constraints:

```
Σ_i Σ_j Σ_k (cost[i,j] × x[i,j,k]) ≤ MaxBudget
```

### Objective function

The solver minimises a weighted sum of normalised metrics. Each metric is first scaled to the `[0, 1]` range based on the actual min/max values across all provider estimates in this request:

```
minimise:
  Σ_i Σ_j Σ_k x[i,j,k] × (
      w_cost      × Normalize(cost[i,j])      +
      w_emissions × Normalize(emissions[i,j]) -
      w_quality   × quality[i,j]              // minus: higher quality is better
  )
  + w_time × Normalize(end[last])             // makespan: total timeline length
```

The step generates **four strategies** by running the solver four times with different weight vectors:

| Priority | `w_cost` | `w_time` | `w_quality` | `w_emissions` |
|---|---|---|---|---|
| `LowestCost` | 0.8 | 0.1 | 0.05 | 0.05 |
| `FastestDelivery` | 0.1 | 0.8 | 0.05 | 0.05 |
| `HighestQuality` | 0.2 | 0.2 | 0.5 | 0.1 |
| `LowestEmissions` | 0.1 | 0.1 | 0.2 | 0.6 |

Each run is independent — same variables and constraints, different objective weights. If a solve returns `OPTIMAL` or `FEASIBLE`, the result is converted into a `StrategyModel` and added to `context.Plan.Strategies`. An `INFEASIBLE` or `NOT_SOLVED` result for a particular priority is silently skipped (that strategy is not generated). If no priority yields a feasible solution, the step throws `OptimizationException`.

---

## Phase 3 — Extracting the result

After solving, `ExtractMipResult` reads the solution values. For each step `i`, exactly one `x[i,j,k]` variable will have `SolutionValue() > 0.5`. This identifies the selected provider and the concrete time slot (with its actual schedule segments). The extracted data forms a `ScheduleTimeline` — an ordered list of `ScheduledProcess` records, each with the chosen provider and the `AllocatedSchedule` (the actual segment sequence from the selected slot).

After all strategies are assembled, `ClampSchedulesToWorkingTimeline()` trims any allocated schedule segments to stay strictly within the working portions of the provider's calendar.

---

## Summary

`OptimizationStep` does three things in sequence:

1. **Enumerate the solution space** — convert provider schedules and estimates into indexed time slots indexed by `(step, provider, slot)`.
2. **Formulate and solve MIP** — four times, once per priority, with sequential and timing constraints that ensure the resulting plan is physically executable.
3. **Extract strategies** — translate solver output back into domain objects containing selected providers, concrete schedules, and aggregate metrics.

The result of this step is `context.Plan.Strategies` — a list of up to four ready-to-present optimization strategies. The next step (`StrategySelectionStep`) publishes them and waits for the user to pick one.
