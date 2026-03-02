using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ManufacturingOptimization.Gateway.Data;

/// <summary>
/// Design-time factory for creating GatewayDbContext during migrations.
/// This allows EF Core tools to create the DbContext without starting the full application.
/// </summary>
public class GatewayDbContextFactory : IDesignTimeDbContextFactory<GatewayDbContext>
{
    public GatewayDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GatewayDbContext>();
        
        // Use SQLite with a design-time connection string matching runtime path
        var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
        Directory.CreateDirectory(dataDir);
        var dbPath = Path.Combine(dataDir, "gateway.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        
        return new GatewayDbContext(optionsBuilder.Options);
    }
}
