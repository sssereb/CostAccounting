using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Domain;

namespace PortfolioApp.Application.Repositories.InMemory;

public class InMemoryAssetRepository : IAssetRepository
{
    private readonly Dictionary<Guid, Asset> _byId = new();
    private readonly Dictionary<string, Asset> _byTicker = new(StringComparer.OrdinalIgnoreCase);
    public Asset? Get(Guid id) => _byId.TryGetValue(id, out var a) ? a : null;
    public Asset? GetByTicker(string t) => _byTicker.TryGetValue(t, out var a) ? a : null;
    public Asset Create(string ticker)
    {
        var a = new Asset(ticker);
        _byId[a.Id] = a;
        _byTicker[a.Ticker] = a;
        return a;
    }
    
    public IEnumerable<Asset> GetAll() => _byId.Values;
}