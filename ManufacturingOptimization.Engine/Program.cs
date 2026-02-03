using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Mappings;
using ManufacturingOptimization.Common.Models.Data.Repositories;
using ManufacturingOptimization.Engine;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Data;
using ManufacturingOptimization.Engine.Handlers;
using ManufacturingOptimization.Engine.Services;
using ManufacturingOptimization.Engine.Services.Pipeline;
using ManufacturingOptimization.Engine.Settings;

var builder = Host.CreateApplicationBuilder(args);

// Configure SQLite database
builder.Services.AddDatabase();

// Register repositories
builder.Services.AddScoped<IProviderRepository, ProviderRepository>();

// Database lifecycle management
builder.Services.AddHostedService<DatabaseManagementService>();

// Add AutoMapper
builder.Services.AddAutoMapper(c =>
{
    c.AddProfile<ProviderMappingProfile>();
    c.AddProfile<OptimizationMappingProfile>();
});

// Configure RabbitMQ
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));
builder.Services.Configure<ProviderValidationSettings>(builder.Configuration.GetSection(ProviderValidationSettings.SectionName));

builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessageSubscriber>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessagingInfrastructure>(sp => sp.GetRequiredService<RabbitMqService>());

// System readiness coordination
builder.Services.Configure<SystemReadinessSettings>(o => o.ServiceName = "Engine");
builder.Services.AddSingleton<ISystemReadinessService, StartupCoordinator>();
builder.Services.AddHostedService(sp => (StartupCoordinator)sp.GetRequiredService<ISystemReadinessService>());

// Message dispatching
builder.Services.AddSingleton<IMessageDispatcher, MessageDispatcher>();
builder.Services.AddScoped<IMessageHandler<ProviderRegisteredEvent>, ProviderRegisteredHandler>();
builder.Services.AddScoped<IMessageHandler<RequestOptimizationPlanCommand>, OptimizationRequestHandler>();

// Pipeline factory
builder.Services.AddSingleton<IWorkflowPipelineFactory, PipelineFactory>();

builder.Services.AddHostedService<EngineWorker>();

var host = builder.Build();
host.Run();
