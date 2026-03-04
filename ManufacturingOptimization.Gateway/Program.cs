using ManufacturingOptimization.Common.Abstractions;
using ManufacturingOptimization.Common.Messages;
using ManufacturingOptimization.Common.Services;
using ManufacturingOptimization.Common.Settings;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Abstractions.Repositories;
using ManufacturingOptimization.Gateway.Abstractions.Services;
using ManufacturingOptimization.Gateway.Data;
using ManufacturingOptimization.Gateway.Data.Repositories;
using ManufacturingOptimization.Gateway.Extensions;
using ManufacturingOptimization.Gateway.Handlers;
using ManufacturingOptimization.Gateway.Middleware;
using ManufacturingOptimization.Gateway.Services;
using ManufacturingOptimization.Gateway.Settings;

var builder = WebApplication.CreateBuilder(args);

// Configure SQLite database
builder.Services.AddDatabase();

// Register repositories
builder.Services.AddScoped<IProviderRepository, ProviderRepository>();
builder.Services.AddScoped<IOptimizationPlanRepository, OptimizationPlanRepository>();
builder.Services.AddScoped<IOptimizationStrategyRepository, OptimizationStrategyRepository>();
builder.Services.AddScoped<IOptimizationRequestRepository, OptimizationRequestRepository>();
builder.Services.AddScoped<IProviderScheduleRepository, ProviderScheduleRepository>();
builder.Services.AddScoped<IProcessEstimateRepository, ProcessEstimateRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IAlternativeProvidersRepository, InMemoryAlternativeProvidersRepository>();

// Database lifecycle management
builder.Services.AddHostedService<DatabaseManagementService>();

// Add CORS
builder.Services.AddCorsConfiguration();

// Add Services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(); // Required for Legacy "Get Providers"

// Configure AutoMapper with unified profile
builder.Services.AddAutoMapper(c =>
{
    c.AddProfile<GatewayMappingProfile>();
});

// Configure settings from environment
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));
builder.Services.Configure<OrchestrationSettings>(builder.Configuration.GetSection(OrchestrationSettings.SectionName));
builder.Services.Configure<DockerSettings>(builder.Configuration.GetSection(DockerSettings.SectionName));

// Register appropriate orchestrator based on mode
builder.Services.AddSingleton<IProviderOrchestrator, DockerProviderOrchestrator>();

// Register RabbitMQ Service
builder.Services.AddSingleton<RabbitMqService>();

// Register Async Awaiter
builder.Services.AddSingleton<IAsyncAwaiter, AsyncAwaiter>();

// Map Messaging Interfaces
builder.Services.AddSingleton<IMessagingInfrastructure>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessageSubscriber>(sp => sp.GetRequiredService<RabbitMqService>());

// Notification Publisher Helper
builder.Services.AddSingleton<INotificationPublisher, NotificationPublisher>();

// Simulation Clock
builder.Services.AddSingleton<ISimulationClock, SimulationClock>();
builder.Services.AddScoped<ISimulationTimeService, SimulationTimeService>();

// Add SignalR
builder.Services.AddSignalR();

// Message dispatching
builder.Services.AddSingleton<IMessageDispatcher, MessageDispatcher>();
builder.Services.AddScoped<IMessageHandler<ProcessExecutionStartedEvent>, ProcessExecutionStartedEventHandler>();
builder.Services.AddScoped<IMessageHandler<ProcessExecutionCompletedEvent>, ProcessExecutionCompletedEventHandler>();
builder.Services.AddScoped<IMessageHandler<ServiceReadyEvent>, ServiceReadyHandler>();
builder.Services.AddScoped<IMessageHandler<SystemReadyEvent>, SystemReadyHandler>();
builder.Services.AddScoped<IMessageHandler<StartAllProvidersCommand>, StartAllProvidersHandler>();
builder.Services.AddScoped<IMessageHandler<AllProvidersStartedEvent>, AllProvidersStartedHandler>();
builder.Services.AddScoped<IMessageHandler<StopAllProvidersCommand>, StopAllProvidersHandler>();
builder.Services.AddScoped<IMessageHandler<AllProvidersStoppedEvent>, AllProvidersStoppedHandler>();
builder.Services.AddScoped<IMessageHandler<StartProviderCommand>, StartProviderHandler>();
builder.Services.AddScoped<IMessageHandler<ProviderStartedEvent>, ProviderStartedHandler>();
builder.Services.AddScoped<IMessageHandler<StopProviderCommand>, StopProviderHandler>();
builder.Services.AddScoped<IMessageHandler<ProviderStoppedEvent>, ProviderStoppedHandler>();
builder.Services.AddScoped<IMessageHandler<OptimizationPlanUpdatedEvent>, OptimizationPlanUpdatedHandler>();
builder.Services.AddScoped<IMessageHandler<CreateNotificationCommand>, CreateNotificationHandler>();

// System readiness coordination
builder.Services.AddSingleton<ISystemReadinessService, SystemReadinessService>();
builder.Services.AddHostedService(sp => (SystemReadinessService)sp.GetRequiredService<ISystemReadinessService>());

// Add Background Worker
builder.Services.AddHostedService<GatewayWorker>();
builder.Services.AddScoped<IOptimizationRequestService, OptimizationRequestService>();
builder.Services.AddScoped<IProviderService, ProviderService>();
builder.Services.AddScoped<IOptimizationPlanService, OptimizationPlanService>();
builder.Services.AddScoped<IOptimizationStrategyService, OptimizationStrategyService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IExecutionStatusService, ExecutionStatusService>();

var app = builder.Build();

// Configure Pipeline
app.UseSwagger();
app.UseSwaggerUI();

// Add CORS middleware (must be before other middleware)
app.UseCors("AllowFrontend");

// Add middleware
app.UseExceptionHandling();
app.UseSystemReadiness();

app.UseAuthorization();
app.MapControllers();

app.Run();