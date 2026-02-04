using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.ProviderRegistry.Data;
using ManufacturingOptimization.ProviderSimulator;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Mappings;
using ManufacturingOptimization.ProviderSimulator.Data.Repositories;
using ManufacturingOptimization.ProviderSimulator.Handlers;
using ManufacturingOptimization.ProviderSimulator.Models;
using ManufacturingOptimization.ProviderSimulator.Services;
using ManufacturingOptimization.ProviderSimulator.Settings;

var builder = Host.CreateApplicationBuilder(args);

// Configure SQLite database
builder.Services.AddDatabase();

// Register repositories (Singleton to support Singleton IProviderSimulator)
builder.Services.AddScoped<IExecutionRepository, ExecutionRepository>();
builder.Services.AddScoped<IProposalRepository, ProposalRepository>();

// Database lifecycle management
builder.Services.AddHostedService<DatabaseManagementService>();

// Database lifecycle management
builder.Services.AddAutoMapper(c =>
{
    c.AddProfile<MotorSpecificationsMappingProfile>();
    c.AddProfile<ProposalMappingProfile>();
    c.AddProfile<EstimateMappingProfile>();
    c.AddProfile<ExecutionScheduleSegmentMappingProfile>();
});

// Configure RabbitMQ
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(RabbitMqSettings.SectionName));

builder.Services.Configure<ProcessStandardsSettings>(builder.Configuration.GetSection(ProcessStandardsSettings.SectionName));
builder.Services.Configure<ProviderSettings>(builder.Configuration.GetSection(ProviderSettings.SectionName));

// Post-configure ProviderSettings to parse WorkingDays from comma-separated environment variable
builder.Services.PostConfigure<ProviderSettings>(options =>
{
    var workingDaysEnv = Environment.GetEnvironmentVariable("Provider__WorkingHours__WorkingDays");
    if (!string.IsNullOrWhiteSpace(workingDaysEnv))
    {
        var days = workingDaysEnv.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(d => (DayOfWeek)int.Parse(d.Trim()))
            .ToHashSet();
        options.WorkingHours.WorkingDays = days;
    }
});

builder.Services.Configure<ProviderTypeSettings>(options =>
{
    options.Type = Environment.GetEnvironmentVariable("PROVIDER_TYPE") ?? string.Empty;
});

builder.Services.AddSingleton<RabbitMqService>();
builder.Services.AddSingleton<IMessagePublisher>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessageSubscriber>(sp => sp.GetRequiredService<RabbitMqService>());
builder.Services.AddSingleton<IMessagingInfrastructure>(sp => sp.GetRequiredService<RabbitMqService>());

// Message dispatching
builder.Services.AddSingleton<IMessageDispatcher, MessageDispatcher>();
builder.Services.AddScoped<IMessageHandler<ProposeProcessToProviderCommand>, ProcessProposalHandler>();
builder.Services.AddScoped<IMessageHandler<ConfirmProcessProposalCommand>, ProcessConfirmationHandler>();
builder.Services.AddScoped<IMessageHandler<UpdateProviderCommand>, UpdateProviderHandler>();
builder.Services.AddScoped<IMessageHandler<RequestProviderScheduleCommand>, RequestProviderScheduleHandler>();

// Register provider simulator
builder.Services.AddSingleton<IProviderSimulationContext, ProviderSimulationContext>();

builder.Services.AddHostedService<ProviderSimulatorWorker>();

var host = builder.Build();

host.Run();
