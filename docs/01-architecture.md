# System Architecture

## Projects

The solution consists of three services and one shared library.

### ManufacturingOptimization.Gateway

The main backend service. It is the only entry point for the frontend: all HTTP requests go through it. The Gateway is responsible for:

- Managing providers — storing their data in a SQLite database and starting/stopping their Docker containers.
- Receiving and storing optimization requests, plans, and strategies.
- Coordinating the full optimization workflow: it publishes commands to RabbitMQ, waits for responses from the Engine and providers, and updates state accordingly.

The Gateway uses [`GatewayWorker`](../ManufacturingOptimization.Gateway/GatewayWorker.cs) as a background service to set up RabbitMQ subscriptions and handle incoming events. Message handling is done through a dispatcher pattern — each event type has a dedicated `IMessageHandler<T>` registered in DI (e.g. `ProcessExecutionStartedEventHandler`, `OptimizationPlanUpdatedHandler`).

### ManufacturingOptimization.Engine

The optimization engine. It runs as a background worker, listens for optimization requests from the Gateway, selects the appropriate strategy (Upgrade or Refurbish) based on the motor specifications, generates an optimization plan with assigned process steps and providers, and publishes the result back.

### ManufacturingOptimization.ProviderSimulator

A simulator of a single technology provider (a manufacturing company). The project is designed to run as multiple independent Docker containers — one container per provider. The number of providers is not fixed.

Each container receives its entire configuration through environment variables at startup: provider ID, name, process capabilities (with cost, speed, quality, and energy parameters), technical capabilities, working hours, break schedules, and RabbitMQ connection settings. There is no configuration file — the container is fully parameterized at launch.

Once started, the simulator connects to RabbitMQ, registers itself as ready, and begins responding to proposals from the Gateway. It also runs an [`ExecutionSchedulerService`](../ManufacturingOptimization.ProviderSimulator/Services/ExecutionSchedulerService.cs) background worker that monitors the simulation clock and starts or completes executions at the scheduled times.

All provider data — proposals, estimates, confirmed executions and their schedule segments — is stored in a shared SQLite database mounted via a Docker volume (`provider_simulator_data`).

### ManufacturingOptimization.Common

A shared library used by all three services. It contains:

- **RabbitMQ infrastructure** — [`RabbitMqService`](../ManufacturingOptimization.Common.Messaging/RabbitMqService.cs) (implements `IMessagePublisher`, `IMessageSubscriber`, `IMessagingInfrastructure`), [`AsyncAwaiter`](../ManufacturingOptimization.Common.Messaging/AsyncAwaiter.cs) for request-reply patterns, [`MessageDispatcher`](../ManufacturingOptimization.Common.Messaging/MessageDispatcher.cs) for routing incoming messages to handlers.
- **Abstractions** — interfaces like `IMessage`, `IMessageHandler<T>`, `ISimulationClock`, `ISystemReadinessService`, `IRepository<T>`, etc.
- **Messages** — all RabbitMQ message types and routing key constants (process proposals, confirmations, execution events, system time, etc.).
- **Contracts** — shared data models used across service boundaries (`MotorSpecificationsModel`, `ProviderScheduleModel`, `ProcessEstimateModel`, etc.).
- **Services** — `SimulationClock`, [`SystemReadinessService`](../ManufacturingOptimization.Common.Messaging/SystemReadinessService.cs), `NotificationPublisher`, base `Repository<T>`.
- **Extensions** — schedule segment manipulation logic (`TryBuildWorkSlot`, `Overlay`, `Subtract`, etc.), working hours helpers.

---

## Docker Container Orchestration

### How providers are started

When the Gateway starts a provider, [`DockerProviderOrchestrator`](../ManufacturingOptimization.Gateway/Services/DockerProviderOrchestrator.cs).`StartAsync` creates a new container from the pre-built `provider-simulator` image. The container name is `provider-{id}`. Its entire configuration is passed as environment variables built from the `ProviderEntity` stored in the Gateway's database — including all process capabilities, technical specs, working hours, and RabbitMQ settings. The container is set to auto-remove on stop.

All provider containers share a single Docker volume (`provider_simulator_data`) that contains their SQLite database, so data persists across container restarts.

The network the container joins is detected automatically: if the Gateway itself is running inside Docker, the orchestrator inspects its own container to find the current network name and attaches new providers to the same network. On a local dev machine (no container context), it falls back to `bridge`.

On startup, the orchestrator also cleans up any leftover containers from a previous session — it identifies them by the `orchestration-mode=production` Docker label.

### Two startup modes

The active mode is controlled by the `Orchestration__Mode` environment variable on the Gateway service, which populates [`OrchestrationSettings`](../ManufacturingOptimization.Gateway/Settings/OrchestrationSettings.cs). The value is either `Production` or `Development`.

**Production mode** — the Gateway's [`DockerProviderOrchestrator`](../ManufacturingOptimization.Gateway/Services/DockerProviderOrchestrator.cs) creates and manages provider containers dynamically at runtime via the Docker API. Before starting the system, the provider image must be built using the `build-provider-image.ps1` script.

**Development mode** — instead of dynamic orchestration, a Docker Compose override file (`docker-compose.dev.yml`, renamed to `docker-compose.override.yml`) statically defines three provider containers with all their environment variables hardcoded. The Gateway is set to `Orchestration__Mode=Development`, which disables dynamic orchestration. This mode is used when debugging the provider code directly.
