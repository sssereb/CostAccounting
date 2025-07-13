
namespace PortfolioApp.Application;
using PortfolioApp.Domain;

internal static class SellHelper
{
    internal static SaleResult SellFromOrderedLots(IEnumerable<Lot> orderedLots, int qty, decimal price)
    {
        int need = qty; decimal costSold = 0, profit = 0;
        foreach (var lot in orderedLots)
        {
            if (need == 0) break;
            int take = Math.Min(lot.QtyRemain, need);
            need -= take;
            lot.QtyRemain -= take;
            costSold += take * lot.UnitCost;
            profit += take * (price - lot.UnitCost);
        }
        if (need > 0) throw new InvalidOperationException("Not enough shares");
        int remQty = orderedLots.Sum(l => l.QtyRemain);
        decimal remCost = orderedLots.Sum(l => l.QtyRemain * l.UnitCost);
        return new SaleResult(remQty, costSold / qty, remQty == 0 ? 0 : remCost / remQty, profit);
    }
}
