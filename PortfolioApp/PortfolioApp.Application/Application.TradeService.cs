using PortfolioApp.Application;
using PortfolioApp.Application.Fees;
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
    private readonly IFeeService _fees;
    public TradeService(IAssetRepository assets, ITradeRepository trades, ILotRepository lots, ICostBasisFactory factory, IFeeService fees)
    {
        _assets = assets; _trades = trades; _lots = lots; _factory = factory; _fees = fees;
    }
    public void Buy(Guid assetId, int qty, decimal price, DateTime date )
    {
        var asset = _assets.Get(assetId) ?? throw new InvalidOperationException("Asset not found");
        var fees = _fees.CalcAll(qty, price, FeeDirection.Buy);
        var totalFee = fees.Sum(f => f.Amount);
        var pricePerShare = price + totalFee / qty;

        var lot = new Lot(assetId, date, qty, rawUnitCost: price, unitCostIncludingFees: pricePerShare);
        var trade = new Trade(Guid.NewGuid(), assetId, date, qty, price, fees);
        
        _trades.Add(trade);
        _lots.Save(lot);
    }
    public SaleResult Sell(Guid assetId, int qty, decimal price, CostBasisMethod method, DateTime date)
    {
        var lots = _lots.GetForAsset(assetId).ToList();
        var res = _factory.Get(method).Sell(lots, qty, price);
        var fees = _fees.CalcAll(qty, price, FeeDirection.Sell);
        var totalFee = fees.Sum(f => f.Amount);
        foreach (var l in lots) _lots.Save(l);
        _trades.Add(new Trade(Guid.NewGuid(), assetId, date, -qty, price, fees));
        return res with
        {
            NetProfit = res.GrossProfit - totalFee
        };
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
