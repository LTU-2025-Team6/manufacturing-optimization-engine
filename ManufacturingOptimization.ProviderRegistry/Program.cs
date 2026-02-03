using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Mappings;
using ManufacturingOptimization.Common.Models.Data.Repositories;
using ManufacturingOptimization.ProviderRegistry;
using ManufacturingOptimization.ProviderRegistry.Abstractions;
using ManufacturingOptimization.ProviderRegistry.Data;
using ManufacturingOptimization.ProviderRegistry.Services;

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
});

// Configure OrchestrationSettings
builder.Services.Configure<OrchestrationSettings>(builder.Configuration.GetSection(OrchestrationSettings.SectionName));

// Configure DockerSettings
builder.Services.Configure<DockerSettings>(builder.Configuration.GetSection(DockerSettings.SectionName));

// Configure RabbitMQ
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));

builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessageSubscriber>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessagingInfrastructure>(sp => sp.GetRequiredService<RabbitMqService>());

// System readiness coordination
builder.Services.Configure<SystemReadinessSettings>(o => o.ServiceName = "ProviderRegistry");
builder.Services.AddSingleton<ISystemReadinessService, SystemReadinessService>();
builder.Services.AddHostedService(sp => (SystemReadinessService)sp.GetRequiredService<ISystemReadinessService>());

// Provider orchestration services
builder.Services.AddSingleton<IProviderValidationService, ProviderValidationService>();

// Register appropriate orchestrator based on mode
var orchestrationMode = builder.Configuration["Orchestration:Mode"] ?? "Production";
if (orchestrationMode == "Production")
{
    // Production: DockerProviderOrchestrator handles validation + deployment
    builder.Services.AddSingleton<IProviderOrchestrator, DockerProviderOrchestrator>();
}
else
{
    // Development: Simple tracker for 3 compose-managed providers
    builder.Services.AddSingleton<IProviderOrchestrator, ComposeManagedOrchestrator>();
}

// Worker
builder.Services.AddHostedService<ProviderRegistryWorker>();

// Message dispatching
builder.Services.AddSingleton<IMessageDispatcher, MessageDispatcher>();

var host = builder.Build();
//host.Run();
