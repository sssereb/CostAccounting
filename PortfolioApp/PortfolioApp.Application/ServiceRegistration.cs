// Application.ServiceRegistration.cs  (в любом *.Application* файле)

using Microsoft.Extensions.DependencyInjection;
using PortfolioApp.Application.Fees;
using PortfolioApp.Application.Repositories.InMemory;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Application.Strategies;
using PortfolioApp.Domain;
using PortfolioApp.Infrastructure.Fees;

namespace PortfolioApp.Application;

public static class PortfolioServices
{
    public static IServiceCollection AddPortfolioCore(this IServiceCollection services)
    {
        return services
            /* ── Cost-basis strategies ──────────────────────────────── */
            .AddSingleton<ICostBasisStrategy, FifoStrategy>()
            .AddSingleton<ICostBasisStrategy, LifoStrategy>()
            .AddSingleton<ICostBasisStrategy, AverageCostStrategy>()
            .AddSingleton<ICostBasisFactory, CostBasisFactory>()

            /* ── Fee calculators (конкретные + интерфейс) ───────────── */
            .AddSingleton<IFeeRuleProvider>(_ =>
                new MemoryFeeRuleProvider(Array.Empty<FeeRegistration>()))
            .AddSingleton<IFeeService, FeeService>()

            /* ── Repositories ───────────────────────────────────────── */
            .AddSingleton<IAssetRepository, InMemoryAssetRepository>()
            .AddSingleton<ILotRepository,   InMemoryLotRepository>()
            .AddSingleton<ITradeRepository, InMemoryTradeRepository>()

            /* ── Application services ──────────────────────────────── */
            .AddSingleton<TradeService>();
    }
}