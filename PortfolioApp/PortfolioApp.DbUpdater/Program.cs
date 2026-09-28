using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PortfolioApp.Infrastructure.EfCore;

var dbPath = DbPathResolver.Resolve(Environment.GetEnvironmentVariable("DB_PATH"));

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(lb => lb.ClearProviders().AddConsole())
    .ConfigureServices(services => services.AddPortfolioEfCoreSqlite(dbPath))
    .Build();

using var scope = host.Services.CreateScope();
var migrator = scope.ServiceProvider.GetRequiredService<IDbMigrator>();
await migrator.MigrateAsync();              // creates the database file and tables from migrations

Console.WriteLine($"DB created/updated at: {dbPath}");
