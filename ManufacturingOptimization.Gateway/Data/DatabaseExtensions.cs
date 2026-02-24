using ManufacturingOptimization.Common.Models.Data.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Gateway.Data;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDir);
        var dbPath = Path.Combine(dataDir, "gateway.db");

        services.AddDbContext<GatewayDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));
        
        services.AddScoped<IDbContext, GatewayDbContext>();
        services.AddScoped<IProviderDbContext, GatewayDbContext>();
        services.AddScoped<IOptimizationDbContext, GatewayDbContext>();

        return services;
    }
}
