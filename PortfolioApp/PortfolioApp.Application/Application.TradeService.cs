using PortfolioApp.Application.Fees;
using PortfolioApp.Application.Persistence;
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
    private readonly IUnitOfWork _uow;

    public TradeService(
        IAssetRepository assets,
        ITradeRepository trades,
        ILotRepository lots,
        ICostBasisFactory factory,
        IFeeService fees,
        IUnitOfWork uow)
    {
        _assets = assets; _trades = trades; _lots = lots; _factory = factory; _fees = fees; _uow = uow;
    }

    public async Task BuyAsync(Guid assetId, int qty, decimal price, DateTime date, CancellationToken ct = default)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        if (price <= 0) throw new ArgumentOutOfRangeException(nameof(price));

        var asset = await _assets.GetAsync(assetId, ct) ?? throw new InvalidOperationException("Asset not found");

        var fees = _fees.CalcAll(qty, price, FeeDirection.Buy).ToList();
        var totalFee = fees.Sum(f => f.Amount);
        var pricePerShare = price + totalFee / qty;

        var lot = new Lot(assetId, date, qty, rawUnitCost: price, unitCostIncludingFees: pricePerShare);
        var trade = new Trade(Guid.NewGuid(), assetId, date, qty, price, 0,0, fees);

        await _uow.ExecuteInTransactionAsync(async token =>
        {
            await _trades.AddAsync(trade, token);
            await _lots.SaveAsync(lot, token);
        }, ct);
    }

    public async Task<SaleResult> SellAsync(Guid assetId, int qty, decimal price, CostBasisMethod method, DateTime date, CancellationToken ct = default)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        if (price <= 0) throw new ArgumentOutOfRangeException(nameof(price));

        var lots = (await _lots.GetForAssetAsync(assetId, ct)).ToList();
        if (lots.Sum(l => l.QtyRemain) < qty) throw new InvalidOperationException("Not enough shares to sell.");

        var res = _factory.Get(method).Sell(lots, qty, price);

        var fees = _fees.CalcAll(qty, price, FeeDirection.Sell).ToList();
        var totalFee = fees.Sum(f => f.Amount);

        var trade = new Trade(Guid.NewGuid(), assetId, date, -qty, price, res.GrossProfit, res.GrossProfit - totalFee, fees);

        // Lots are read without a transaction; the lot version check at write time detects concurrent sales.
        await _uow.ExecuteInTransactionAsync(async token =>
        {
            foreach (var l in lots)
                await _lots.SaveAsync(l, token);
            await _trades.AddAsync(trade, token);
        }, ct);

        return res with { NetProfit = res.GrossProfit - totalFee };
    }

    public async Task<decimal> GetRemainingCostPerShareAsync(Guid assetId, CancellationToken ct = default)
    {
        var lots = (await _lots.GetForAssetAsync(assetId, ct)).ToList();
        var remainingShares = lots.Sum(l => l.QtyRemain);
        if (remainingShares == 0) return 0m;
        var totalCost = lots.Sum(l => l.QtyRemain * l.UnitCost);
        return totalCost / remainingShares;
    }
}
