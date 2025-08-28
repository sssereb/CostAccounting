using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PortfolioApp.Infrastructure.EfCore;

public sealed class EfDbMigrator : IDbMigrator
{
    private readonly PortfolioDbContext _db;
    private readonly ILogger<EfDbMigrator> _log;

    public EfDbMigrator(PortfolioDbContext db, ILogger<EfDbMigrator> log)
    {
        _db = db;
        _log = log;
    }

    public async Task MigrateAsync(CancellationToken ct = default)
    {
        _log.LogInformation("Applying EF Core migrations…");
        await _db.Database.MigrateAsync(ct);      
        _log.LogInformation("Migrations applied.");

    }
}