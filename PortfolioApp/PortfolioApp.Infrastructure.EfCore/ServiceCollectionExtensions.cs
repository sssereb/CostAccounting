using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Infrastructure.EfCore.Repositories;

namespace PortfolioApp.Infrastructure.EfCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPortfolioRepositoriesEfCore(this IServiceCollection services)
    {
        services.AddScoped<IAssetRepository, EfAssetRepository>();
        services.AddScoped<ILotRepository,   EfLotRepository>();
        services.AddScoped<ITradeRepository, EfTradeRepository>();
        return services;
    }
    
    public static IServiceCollection AddPortfolioEfCoreSqlite(this IServiceCollection services)
    {
        var dbPath = DbPathResolver.getAbsoluteDbPath(); // resolved from config only

        var cs = new SqliteConnectionStringBuilder { DataSource = dbPath, Cache = SqliteCacheMode.Shared }.ToString();

        services.AddDbContext<PortfolioDbContext>(opt => opt.UseSqlite(cs));

        services.AddScoped<IDbMigrator, EfDbMigrator>();

        return services;
    }
}