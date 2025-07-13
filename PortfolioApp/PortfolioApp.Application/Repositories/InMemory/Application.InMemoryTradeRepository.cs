
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Domain;

namespace PortfolioApp.Application.Repositories.InMemory;

public class InMemoryTradeRepository : ITradeRepository
{
    private readonly List<Trade> _trades = new();
    public IEnumerable<Trade> GetForAsset(Guid assetId) => _trades.Where(t => t.AssetId == assetId);
    public void Add(Trade trade) => _trades.Add(trade);
}