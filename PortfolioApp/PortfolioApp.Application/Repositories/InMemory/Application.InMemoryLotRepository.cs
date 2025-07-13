
namespace PortfolioApp.Application;
using PortfolioApp.Domain;

public class InMemoryLotRepository : ILotRepository
{
    private readonly List<Lot> _lots = new();
    public IEnumerable<Lot> GetForAsset(Guid assetId) => _lots.Where(l => l.AssetId == assetId);
    public void Save(Lot lot)
    {
        var idx = _lots.FindIndex(l => l.Id == lot.Id);
        if (idx >= 0) _lots[idx] = lot; else _lots.Add(lot);
    }
}