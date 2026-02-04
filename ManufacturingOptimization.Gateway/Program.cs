using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Mappings;
using ManufacturingOptimization.Common.Models.Data.Repositories;
using ManufacturingOptimization.Gateway.Abstractions;
using ManufacturingOptimization.Gateway.Data;
using ManufacturingOptimization.Gateway.Extensions;
using ManufacturingOptimization.Gateway.Handlers;
using ManufacturingOptimization.Gateway.Middleware;
using ManufacturingOptimization.Gateway.Services;
using ManufacturingOptimization.Gateway.Settings;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Configure SQLite database
builder.Services.AddDatabase();

// Register repositories
builder.Services.AddScoped<IProviderRepository, ProviderRepository>();
builder.Services.AddScoped<IOptimizationPlanRepository, OptimizationPlanRepository>();
builder.Services.AddScoped<IOptimizationStrategyRepository, OptimizationStrategyRepository>();

// Database lifecycle management
builder.Services.AddHostedService<DatabaseManagementService>();

// Add CORS
builder.Services.AddCorsConfiguration();

// Add Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(); // Required for Legacy "Get Providers"

// Add AutoMapper
// Add AutoMapper
builder.Services.AddAutoMapper(c =>
{
    c.AddProfile<ProviderMappingProfile>();
    c.AddProfile<OptimizationMappingProfile>();
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

// Message dispatching
builder.Services.AddSingleton<IMessageDispatcher, MessageDispatcher>();
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

// System readiness coordination
builder.Services.Configure<SystemReadinessSettings>(o => o.ServiceName = "Gateway");
builder.Services.AddSingleton<ISystemReadinessService, SystemReadinessService>();
builder.Services.AddHostedService(sp => (SystemReadinessService)sp.GetRequiredService<ISystemReadinessService>());

// Add Background Worker
builder.Services.AddHostedService<GatewayWorker>();
builder.Services.AddScoped<IOptimizationService, OptimizationService>();
builder.Services.AddScoped<IProviderService, ProviderService>();

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