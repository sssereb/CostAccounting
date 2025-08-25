using PortfolioApp.Domain;
using System.Threading;

namespace PortfolioApp.Application.Repositories.Interfaces;

public interface IAssetRepository
{
    Task<Asset?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Asset?> GetByTickerAsync(string ticker, CancellationToken ct = default);
    Task<Asset>  CreateAsync(string ticker, CancellationToken ct = default);
    Task<IReadOnlyList<Asset>> GetAllAsync(CancellationToken ct = default);

    // удобный хелпер, как у тебя было, только async
    public static async Task<Asset> GetOrCreateAsync(IAssetRepository repo, string ticker, CancellationToken ct = default) =>
        (await repo.GetByTickerAsync(ticker, ct)) ?? await repo.CreateAsync(ticker, ct);
}