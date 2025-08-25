using System.Collections.Concurrent;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Domain;

namespace PortfolioApp.Application.Repositories.InMemory;

public class InMemoryAssetRepository : IAssetRepository
{
    // потокобезопасные коллекции (на всякий случай)
    private readonly ConcurrentDictionary<Guid, Asset> _byId = new();
    private readonly ConcurrentDictionary<string, Asset> _byTicker =
        new(StringComparer.OrdinalIgnoreCase);

    public Task<Asset?> GetAsync(Guid id, CancellationToken ct = default)
    {
        _byId.TryGetValue(id, out var a);
        return Task.FromResult(a);
    }

    public Task<Asset?> GetByTickerAsync(string ticker, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ticker)) return Task.FromResult<Asset?>(null);
        _byTicker.TryGetValue(ticker, out var a);
        return Task.FromResult(a);
    }

    public Task<Asset> CreateAsync(string ticker, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ticker))
            throw new ArgumentException("Ticker must be non-empty.", nameof(ticker));

        var t = ticker.Trim().ToUpperInvariant();

        // если уже есть — возвращаем существующий
        var existing = _byTicker.Values.FirstOrDefault(a => a.Ticker.Equals(t, StringComparison.OrdinalIgnoreCase));
        if (existing is not null) return Task.FromResult(existing);

        var asset = new Asset(t);
        _byId[asset.Id] = asset;
        _byTicker[asset.Ticker] = asset;
        return Task.FromResult(asset);
    }

    public Task<IReadOnlyList<Asset>> GetAllAsync(CancellationToken ct = default)
    {
        var list = _byId.Values.OrderBy(a => a.Ticker).ToList();
        return Task.FromResult((IReadOnlyList<Asset>)list);
    }
}