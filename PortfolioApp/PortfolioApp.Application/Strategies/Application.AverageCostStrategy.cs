namespace PortfolioApp.Application.Strategies;
using PortfolioApp.Domain;
public sealed class AverageCostStrategy : CostBasisStrategyBase
{
    public override CostBasisMethod Method => CostBasisMethod.Average;

    // Cost is pooled, so the order only decides which lots' quantities (and dates) are consumed.
    protected override IOrderedEnumerable<Lot> OrderLots(IEnumerable<Lot> lots) =>
        lots.OrderBy(l => l.PurchaseDate);

    public override SaleResult Sell(IList<Lot> lots, int qty, decimal sellPrice, decimal sellFees)
    {
        int totalAvail = lots.Sum(l => l.QtyRemain);
        if (qty > totalAvail)
            throw new InvalidOperationException("Not enough shares.");

        decimal avgCost    = lots.Sum(l => l.QtyRemain * l.UnitCost) / totalAvail;
        decimal avgRawCost = lots.Sum(l => l.QtyRemain * l.RawUnitCost) / totalAvail;

        int need = qty;
        foreach (var lot in OrderLots(lots))
        {
            if (need == 0) break;
            int take = Math.Min(lot.QtyRemain, need);
            lot.QtyRemain -= take;
            need -= take;
        }

        // The shares left keep the pooled cost, so a later FIFO/LIFO sale uses the same basis.
        foreach (var lot in lots.Where(l => l.QtyRemain > 0))
            lot.ApplyAverageCost(avgRawCost, avgCost);

        int remaining = totalAvail - qty;

        return new SaleResult(remaining,
            SoldCostPerShare: avgCost,
            RemainingCostPerShare: remaining == 0 ? 0 : avgCost,
            GrossProfit: qty * (sellPrice - avgRawCost),
            NetProfit:   qty * (sellPrice - avgCost) - sellFees);
    }
}
