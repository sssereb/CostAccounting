namespace PortfolioApp.Infrastructure.EfCore;

public interface IDbMigrator
{
    Task MigrateAsync(CancellationToken ct = default);
}