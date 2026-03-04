using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Common.Services;
using ManufacturingOptimization.Common.Settings;
using ManufacturingOptimization.Engine;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Handlers;
using ManufacturingOptimization.Engine.OptimizationPipeline;
using ManufacturingOptimization.Engine.Repositories;
using ManufacturingOptimization.Engine.Settings;

var builder = Host.CreateApplicationBuilder(args);

// Register repositories
builder.Services.AddSingleton<IProviderRepository, InMemoryProviderRepository>();

// Configure RabbitMQ
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));
builder.Services.Configure<ProviderValidationSettings>(builder.Configuration.GetSection(ProviderValidationSettings.SectionName));

builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessageSubscriber>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessagingInfrastructure>(sp => sp.GetRequiredService<RabbitMqService>());

// Notification Publisher Helper
builder.Services.AddSingleton<INotificationPublisher, NotificationPublisher>();

// Simulation Clock
builder.Services.AddSingleton<ISimulationClock, SimulationClock>();

// Register Async Awaiter
builder.Services.AddSingleton<IAsyncAwaiter, AsyncAwaiter>();

// System readiness coordination
builder.Services.AddSingleton<ISystemReadinessService, SystemReadinessService>();

// Register optimization pipeline steps (Transient - each pipeline gets fresh instances)
builder.Services.AddTransient<WorkflowMatchingStep>();
builder.Services.AddTransient<ProviderMatchingStep>();
builder.Services.AddTransient<EstimationStep>();
builder.Services.AddTransient<OptimizationStep>();
builder.Services.AddTransient<StrategySelectionStep>();
builder.Services.AddTransient<FinalizationStep>();

// Pipeline factory (Singleton - can create pipelines on demand)
builder.Services.AddSingleton<IWorkflowPipelineFactory, PipelineFactory>();

// Message dispatching
builder.Services.AddSingleton<IMessageDispatcher, MessageDispatcher>();
builder.Services.AddScoped<IMessageHandler<ServiceReadyEvent>, ServiceReadyHandler>();
builder.Services.AddScoped<IMessageHandler<SystemReadyEvent>, SystemReadyHandler>();
builder.Services.AddScoped<IMessageHandler<AllProvidersStartedEvent>, AllProvidersStartedHandler>();
builder.Services.AddScoped<IMessageHandler<ProviderStartedEvent>, ProviderStartedHandler>();
builder.Services.AddScoped<IMessageHandler<ProviderStoppedEvent>, ProviderStoppedHandler>();
builder.Services.AddScoped<IMessageHandler<ProviderUpdatedEvent>, ProviderUpdatedHandler>();
builder.Services.AddScoped<IMessageHandler<RequestOptimizationPlanCommand>, OptimizationRequestHandler>();

builder.Services.AddHostedService<EngineWorker>();

var host = builder.Build();
host.Run();
