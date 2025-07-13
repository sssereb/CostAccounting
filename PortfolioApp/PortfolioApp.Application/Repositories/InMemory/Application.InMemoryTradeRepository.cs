
namespace PortfolioApp.Application;
using PortfolioApp.Domain;

public class InMemoryTradeRepository : ITradeRepository
{
    private readonly List<Trade> _trades = new();
    public IEnumerable<Trade> GetForAsset(Guid assetId) => _trades.Where(t => t.AssetId == assetId);
    public void Add(Trade trade) => _trades.Add(trade);
}