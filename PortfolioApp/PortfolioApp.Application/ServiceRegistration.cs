// Application.ServiceRegistration.cs  (в любом *.Application* файле)

using Microsoft.Extensions.DependencyInjection;
using PortfolioApp.Application.Repositories.InMemory;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Application.Strategies;

namespace PortfolioApp.Application;

public static class PortfolioServices
{
    public static IServiceCollection AddPortfolioCore(this IServiceCollection services) =>
        services
            /* стратегии расчёта */
            .AddSingleton<FifoStrategy>()
            .AddSingleton<LifoStrategy>()
            .AddSingleton<AverageCostStrategy>()
            .AddSingleton<ICostBasisFactory, CostBasisFactory>()
            /* репозитории */
            .AddSingleton<IAssetRepository, InMemoryAssetRepository>()
            .AddSingleton<ILotRepository,   InMemoryLotRepository>()
            .AddSingleton<ITradeRepository, InMemoryTradeRepository>()
            /* сервисы */
            .AddSingleton<TradeService>();
}