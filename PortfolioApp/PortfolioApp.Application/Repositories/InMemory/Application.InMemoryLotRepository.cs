using System.Collections.Concurrent;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Domain;

namespace PortfolioApp.Application.Repositories.InMemory;

public class InMemoryLotRepository : ILotRepository
{
    private readonly ConcurrentDictionary<Guid, Lot> _lotsById = new();

    public Task<IReadOnlyList<Lot>> GetForAssetAsync(Guid assetId, CancellationToken ct = default)
    {
        // для предсказуемости сортируем по дате покупки (полезно для FIFO)
        var list = _lotsById.Values
            .Where(l => l.AssetId == assetId)
            .OrderBy(l => l.PurchaseDate)
            .ToList();
        return Task.FromResult((IReadOnlyList<Lot>)list);
    }

    public Task SaveAsync(Lot lot, CancellationToken ct = default)
    {
        _lotsById[lot.Id] = lot;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Lot>> GetAllAsync(CancellationToken ct = default)
    {
        var list = _lotsById.Values
            .OrderBy(l => l.AssetId)
            .ThenBy(l => l.PurchaseDate)
            .ToList();
        return Task.FromResult((IReadOnlyList<Lot>)list);
    }
}