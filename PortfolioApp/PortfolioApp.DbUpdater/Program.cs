using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PortfolioApp.Infrastructure.EfCore;

static string GetDefaultDbPath()
{
    // macOS: ~/Library/Application Support/PortfolioApp/portfolio.db
    // Windows: %LOCALAPPDATA%\PortfolioApp\portfolio.db
    var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    var appDir = Path.Combine(root, "PortfolioApp");
    Directory.CreateDirectory(appDir);
    return Path.Combine(appDir, "portfolio.db");
}

var dbPath  = Environment.GetEnvironmentVariable("DB_PATH") ?? GetDefaultDbPath();

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(lb => lb.ClearProviders().AddConsole())
    .ConfigureServices(services =>
    {
        // DbContext and everything migrations need
        services.AddPortfolioEfCoreSqlite();
    })
    .Build();

using var scope = host.Services.CreateScope();
var migrator = scope.ServiceProvider.GetRequiredService<IDbMigrator>();
await migrator.MigrateAsync();              // creates the database file and tables from migrations

Console.WriteLine($"DB created/updated at: {dbPath}");