using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PortfolioApp.Infrastructure.EfCore;

namespace PortfolioApp.Tests.Support;

/// <summary>SQLite in-memory database with the real migrations applied; lives as long as the connection.</summary>
internal sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly DbContextOptions<PortfolioDbContext> _options;

    public SqliteTestDatabase()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<PortfolioDbContext>().UseSqlite(_connection).Options;
        using var db = CreateContext();
        db.Database.Migrate();
    }

    public PortfolioDbContext CreateContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}
