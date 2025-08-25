using System.Collections.Concurrent;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Domain;

namespace PortfolioApp.Application.Repositories.InMemory;

public sealed class InMemoryTradeRepository : ITradeRepository
{
    // Быстрое добавление/поиск по Id
    private readonly ConcurrentDictionary<Guid, Trade> _byId = new();
    // Индекс по AssetId для быстрых выборок
    private readonly ConcurrentDictionary<Guid, ConcurrentBag<Trade>> _byAsset = new();

    public Task AddAsync(Trade trade, CancellationToken ct = default)
    {
        // upsert по Id
        _byId[trade.Id] = trade;

        var bag = _byAsset.GetOrAdd(trade.AssetId, _ => new ConcurrentBag<Trade>());
        bag.Add(trade);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Trade>> GetForAssetAsync(Guid assetId, CancellationToken ct = default)
    {
        if (_byAsset.TryGetValue(assetId, out var bag))
        {
            // материализуем, сортируем по дате (удобно для отчётов/последовательности)
            var list = bag.OrderBy(t => t.Date).ToList();
            return Task.FromResult((IReadOnlyList<Trade>)list);
        }

        return Task.FromResult((IReadOnlyList<Trade>)Array.Empty<Trade>());
    }

    public Task<IReadOnlyList<Trade>> GetAllAsync(CancellationToken ct = default)
    {
        var list = _byId.Values.OrderBy(t => t.AssetId).ThenBy(t => t.Date).ToList();
        return Task.FromResult((IReadOnlyList<Trade>)list);
    }
}