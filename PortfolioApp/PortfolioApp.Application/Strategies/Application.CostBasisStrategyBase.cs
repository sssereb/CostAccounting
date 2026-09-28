// Application/Strategies/CostBasisStrategyBase.cs
namespace PortfolioApp.Application.Strategies;
using PortfolioApp.Domain;

public abstract class CostBasisStrategyBase : ICostBasisStrategy
{
    public abstract CostBasisMethod Method { get; }

    /// Defines the order in which lots are consumed (FIFO / LIFO / HIFO …)
    protected abstract IOrderedEnumerable<Lot> OrderLots(IEnumerable<Lot> lots);

    /// Template method; average cost overrides it.
    public virtual SaleResult Sell(IList<Lot> lots, int qty, decimal price)
    {
        
        if (qty > lots.Sum(l => l.QtyRemain))
            throw new InvalidOperationException("Not enough shares");
        
        var ordered = OrderLots(lots);

        int need = qty; decimal costSold = 0, profit = 0, gross = 0;
        foreach (var lot in ordered)
        {
            if (need == 0) break;
            int take = Math.Min(lot.QtyRemain, need);
            need -= take;
            lot.QtyRemain -= take;

            costSold += take * lot.UnitCost;
            profit   += take * (price - lot.RawUnitCost);
            gross    += take * (price - lot.UnitCost);
        }

        int     remQty   = lots.Sum(l => l.QtyRemain);
        decimal remCost  = lots.Sum(l => l.QtyRemain * l.UnitCost);

        return new SaleResult(remQty,
            costSold / qty,
            remQty == 0 ? 0 : remCost / remQty,
            GrossProfit: gross,
            NetProfit: 0) ;
    }
}