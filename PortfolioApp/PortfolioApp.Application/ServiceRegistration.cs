using Microsoft.Extensions.DependencyInjection;
using PortfolioApp.Application.Fees;
using PortfolioApp.Application.Persistence;
using PortfolioApp.Application.Repositories.InMemory;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Application.Strategies;
using PortfolioApp.Infrastructure.Fees;

namespace PortfolioApp.Application;

public static class PortfolioServices
{
    public static IServiceCollection AddPortfolioRepositoriesInMemory(this IServiceCollection services)
    {
        services.AddSingleton<IAssetRepository, InMemoryAssetRepository>();
        services.AddSingleton<ILotRepository,   InMemoryLotRepository>();
        services.AddSingleton<ITradeRepository, InMemoryTradeRepository>();
        services.AddSingleton<IUnitOfWork,      NoOpUnitOfWork>();
        return services;
    }
    
    public static IServiceCollection AddPortfolioCore(this IServiceCollection services)
    {
        return services
            /* ── Cost-basis strategies ──────────────────────────────── */
            .AddSingleton<ICostBasisStrategy, FifoStrategy>()
            .AddSingleton<ICostBasisStrategy, LifoStrategy>()
            .AddSingleton<ICostBasisStrategy, AverageCostStrategy>()
            .AddSingleton<ICostBasisFactory, CostBasisFactory>()

            /* ── Fee rules and fee service ─────────────────────────── */
            .AddSingleton<IFeeRuleProvider>(_ =>
                new MemoryFeeRuleProvider(Array.Empty<FeeRegistration>()))
            .AddSingleton<IFeeService, FeeService>()

            /* ── Application services ──────────────────────────────── */
            .AddScoped<TradeService>();
    }
}