
using PortfolioApp.Domain;

namespace PortfolioApp.Application.Strategies;


public sealed class LifoStrategy : CostBasisStrategyBase
{
    public override CostBasisMethod Method => CostBasisMethod.LIFO;
    protected override IOrderedEnumerable<Lot> OrderLots(IEnumerable<Lot> lots) =>
        lots.OrderByDescending(l => l.PurchaseDate);
}