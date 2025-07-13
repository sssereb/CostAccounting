
using PortfolioApp.Domain;

namespace PortfolioApp.Application.Strategies;

public sealed class AverageCostStrategy : ICostBasisStrategy
{
    public SaleResult Sell(IList<Lot> lots, int qty, decimal price)
    {
        int total = lots.Sum(l => l.QtyRemain);
        if (qty > total) throw new InvalidOperationException("Not enough shares");
        decimal avg = lots.Sum(l => l.QtyRemain * l.UnitCost) / total;
        int need = qty;
        foreach (var lot in lots)
        {
            if (need == 0) break;
            int take = Math.Min(lot.QtyRemain, need);
            lot.QtyRemain -= take;
            need -= take;
        }
        int remQty = total - qty;
        return new SaleResult(remQty, avg, remQty == 0 ? 0 : avg, qty * (price - avg));
    }
}
