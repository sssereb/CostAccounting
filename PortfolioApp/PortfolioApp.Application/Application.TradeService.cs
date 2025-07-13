using PortfolioApp.Application;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Application.Strategies;
using PortfolioApp.Domain;
namespace PortfolioApp.Application;

public sealed class TradeService
{
    private readonly IAssetRepository _assets;
    private readonly ITradeRepository _trades;
    private readonly ILotRepository _lots;
    private readonly ICostBasisFactory _factory;
    public TradeService(IAssetRepository assets, ITradeRepository trades, ILotRepository lots, ICostBasisFactory factory)
    {
        _assets = assets; _trades = trades; _lots = lots; _factory = factory;
    }
    public void Buy(Guid assetId, int qty, decimal price, DateTime date)
    {
        var asset = _assets.Get(assetId) ?? throw new("Asset not found");
        _trades.Add(new Trade(Guid.NewGuid(), assetId, date, qty, price));
        _lots.Save(new Lot(assetId, date, qty, price));
    }
    public SaleResult Sell(Guid assetId, int qty, decimal price, CostBasisMethod method, DateTime date)
    {
        var lots = _lots.GetForAsset(assetId).ToList();
        var res = _factory.Get(method).Sell(lots, qty, price);
        foreach (var l in lots) _lots.Save(l);
        _trades.Add(new Trade(Guid.NewGuid(), assetId, date, -qty, price));
        return res;
    }

    public decimal GetRemaininCostPerShare(Guid assetId)
    {
        var lots = _lots.GetForAsset(assetId).ToList();
        var remainingShares = lots.Sum(l => l.QtyRemain);
        if (remainingShares == 0) return 0m;
        var totalCost = lots.Sum(l => l.QtyRemain * l.UnitCost);
        return totalCost / remainingShares;
    }
}
