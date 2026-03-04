using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Common.Abstractions;

public interface IDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
}