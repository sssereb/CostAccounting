// Application.ServiceRegistration.cs  (в любом *.Application* файле)

using Microsoft.Extensions.DependencyInjection;
using PortfolioApp.Application.Fees;
using PortfolioApp.Application.Repositories.InMemory;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Application.Strategies;
using PortfolioApp.Domain;

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
            .AddSingleton<FixedPerTrade>(_ => new(100.00m))
            .AddSingleton<FixedPerShare>(_ => new(0.05m))
            .AddSingleton<PercentOfValue>(_ => new(0.0025m))

            .AddSingleton<IFeeCalculator>(sp => sp.GetRequiredService<FixedPerTrade>())
            .AddSingleton<IFeeCalculator>(sp => sp.GetRequiredService<FixedPerShare>())
            .AddSingleton<IFeeCalculator>(sp => sp.GetRequiredService<PercentOfValue>())

            /* ── Fee registrations (метаданные) ─────────────────────── */
            .AddSingleton<FeeRegistration>(sp =>
                new(sp.GetRequiredService<FixedPerTrade>(),  FeeDirection.Buy))   // тип-1
            .AddSingleton<FeeRegistration>(sp =>
                new(sp.GetRequiredService<FixedPerShare>(),  FeeDirection.Buy))   // тип-2
            .AddSingleton<FeeRegistration>(sp =>
                new(sp.GetRequiredService<PercentOfValue>(), FeeDirection.Sell))  // тип-3

            .AddSingleton<FeeComposite>()

            /* ── Repositories ───────────────────────────────────────── */
            .AddSingleton<IAssetRepository, InMemoryAssetRepository>()
            .AddSingleton<ILotRepository,   InMemoryLotRepository>()
            .AddSingleton<ITradeRepository, InMemoryTradeRepository>()

            /* ── Application services ──────────────────────────────── */
            .AddSingleton<TradeService>();
    }
}