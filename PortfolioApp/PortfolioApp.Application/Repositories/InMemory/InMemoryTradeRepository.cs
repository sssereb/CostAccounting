using System.Collections.Concurrent;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Domain;

namespace PortfolioApp.Application.Repositories.InMemory;

public sealed class InMemoryTradeRepository : ITradeRepository
{
    private readonly ConcurrentDictionary<Guid, Trade> _byId = new();

    public Task AddAsync(Trade trade, CancellationToken ct = default)
    {
        _byId[trade.Id] = trade;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Trade>> GetAllAsync(CancellationToken ct = default)
    {
        var list = _byId.Values.OrderBy(t => t.AssetId).ThenBy(t => t.Date).ToList();
        return Task.FromResult((IReadOnlyList<Trade>)list);
    }
}
