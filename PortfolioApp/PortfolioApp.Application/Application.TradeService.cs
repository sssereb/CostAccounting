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
        EnsurePositive(qty, price);
        await EnsureAssetExistsAsync(assetId, ct);

        await _uow.ExecuteInTransactionAsync(token => RecordBuyAsync(assetId, qty, price, date, token), ct);
    }

    /// <summary>Creates the asset if the ticker is new, in the same transaction as the buy.</summary>
    public async Task BuyByTickerAsync(string ticker, int qty, decimal price, DateTime date, CancellationToken ct = default)
    {
        EnsurePositive(qty, price);

        await _uow.ExecuteInTransactionAsync(async token =>
        {
            var asset = await IAssetRepository.GetOrCreateAsync(_assets, ticker, token);
            await RecordBuyAsync(asset.Id, qty, price, date, token);
        }, ct);
    }

    public async Task<SaleResult> SellAsync(Guid assetId, int qty, decimal price, CostBasisMethod method, DateTime date, CancellationToken ct = default)
    {
        EnsurePositive(qty, price);
        await EnsureAssetExistsAsync(assetId, ct);

        var lots = (await _lots.GetForAssetAsync(assetId, ct)).ToList();
        if (lots.Sum(l => l.QtyRemain) < qty) throw new InvalidOperationException("Not enough shares to sell.");

        var fees = _fees.CalcAll(qty, price, FeeDirection.Sell).ToList();
        var totalFee = fees.Sum(f => f.Amount);

        var res = _factory.Get(method).Sell(lots, qty, price, totalFee);

        var trade = new Trade(Guid.NewGuid(), assetId, date, -qty, price, res.GrossProfit, res.NetProfit, fees);

        // Lots are read without a transaction; the lot version check at write time detects concurrent sales.
        await _uow.ExecuteInTransactionAsync(async token =>
        {
            foreach (var l in lots)
                await _lots.SaveAsync(l, token);
            await _trades.AddAsync(trade, token);
        }, ct);

        return res;
    }

    private async Task RecordBuyAsync(Guid assetId, int qty, decimal price, DateTime date, CancellationToken ct)
    {
        var fees = _fees.CalcAll(qty, price, FeeDirection.Buy).ToList();
        var totalFee = fees.Sum(f => f.Amount);

        var lot = new Lot(assetId, date, qty, rawUnitCost: price, unitCostIncludingFees: price + totalFee / qty);
        var trade = new Trade(Guid.NewGuid(), assetId, date, qty, price, 0, 0, fees);

        await _trades.AddAsync(trade, ct);
        await _lots.SaveAsync(lot, ct);
    }

    private async Task EnsureAssetExistsAsync(Guid assetId, CancellationToken ct)
    {
        if (await _assets.GetAsync(assetId, ct) is null)
            throw new KeyNotFoundException($"Asset {assetId} not found.");
    }

    private static void EnsurePositive(int qty, decimal price)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty), qty, "Quantity must be positive.");
        if (price <= 0) throw new ArgumentOutOfRangeException(nameof(price), price, "Price must be positive.");
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
