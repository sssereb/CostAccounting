using PortfolioApp.Domain;

namespace PortfolioApp.Application.Repositories.Interfaces;

public interface IAssetRepository
{
    Task<Asset?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Asset?> GetByTickerAsync(string ticker, CancellationToken ct = default);
    Task<Asset>  CreateAsync(string ticker, CancellationToken ct = default);
    Task<IReadOnlyList<Asset>> GetAllAsync(CancellationToken ct = default);

    public static async Task<Asset> GetOrCreateAsync(IAssetRepository repo, string ticker, CancellationToken ct = default) =>
        (await repo.GetByTickerAsync(ticker, ct)) ?? await repo.CreateAsync(ticker, ct);
}