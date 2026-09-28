namespace PortfolioApp.Application.Strategies;
using PortfolioApp.Domain;
public sealed class AverageCostStrategy : CostBasisStrategyBase
{
    public override CostBasisMethod Method => CostBasisMethod.Average;

    // Order does not matter for average cost, keep lots as they are
    protected override IOrderedEnumerable<Lot> OrderLots(IEnumerable<Lot> lots) =>
        lots.OrderBy(l => 0);

    public override SaleResult Sell(IList<Lot> lots, int qty, decimal sellPrice)
    {
        int totalAvail = lots.Sum(l => l.QtyRemain);
        if (qty > totalAvail)
            throw new InvalidOperationException("Not enough shares.");

        decimal avgCost = lots.Sum(l => l.QtyRemain * l.UnitCost) / totalAvail;

        int need = qty;
        foreach (var lot in lots)
        {
            if (need == 0) break;
            int take = Math.Min(lot.QtyRemain, need);
            lot.QtyRemain -= take;
            need -= take;
        }

        int     remaining = totalAvail - qty;
        decimal remCostPx = remaining == 0 ? 0 : avgCost;
        decimal profit    = qty * (sellPrice - avgCost);

        return new SaleResult(remaining,
            avgCost,          // cost per sold share
            remCostPx,        // cost basis on remainder
            profit, 
            profit);
    }
}
