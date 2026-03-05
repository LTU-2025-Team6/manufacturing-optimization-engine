# Manufacturing Optimization Engine

Modern manufacturing rarely happens under one roof. When a complex product needs to be built or repaired — say, an industrial electrical motor — the work typically spans multiple specialized companies: one does disassembly, another precision machining, a third redesigns components, a fourth handles certification. Each company has its own schedule, capacity, pricing, and availability. There is no shared system between them, and coordinating the whole process manually is slow and error-prone.

This project is a distributed platform that automates that coordination. Given a customer request, the system figures out which companies can handle each part of the job, collects their availability and cost estimates, and computes an optimized end-to-end production plan — deciding who does what, and when, across all the involved parties. Once the customer approves a plan, the system coordinates execution: it notifies each company when their work should begin, tracks progress, and handles changes or cancellations.

Because the participating companies are independent and geographically distributed, the architecture is inherently distributed as well. Each company runs as a separate node connected to the others through a message broker — they communicate by exchanging structured messages: proposals, acceptances, status updates, and completion confirmations. The system is built on .NET with RabbitMQ for messaging and Docker for deployment.

---

## Documentation

- [Project Overview](docs/project-overview.md) — use case, Upgrade vs Refurbish processes, provider types, backlog
- [System Architecture](docs/01-architecture.md) — services, shared library, Docker orchestration
- [Messaging](docs/02-messaging.md) — RabbitMQ exchanges, routing, AsyncAwaiter request-reply pattern
- [System Readiness](docs/03-system-readiness.md) — two-phase startup coordination across services
- [Optimization Pipeline](docs/04-optimization-pipeline.md) — six-step workflow, WorkflowContext, state machine
- [Optimization Step](docs/05-optimization-step.md) — MIP formulation, OR-Tools SCIP solver, solution space
- [Provider Simulator](docs/06-provider.md) — provider API, plan lifecycle, confirmation and cancellation
- [Execution Lifecycle](docs/07-execution.md) — ExecutionSchedulerService, execution events, monitoring endpoints
