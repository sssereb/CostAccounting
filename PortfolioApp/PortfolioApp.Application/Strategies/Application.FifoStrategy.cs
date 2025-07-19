
using PortfolioApp.Domain;

namespace PortfolioApp.Application.Strategies;

public sealed class FifoStrategy : CostBasisStrategyBase
{
    public override CostBasisMethod Method => CostBasisMethod.FIFO;
    protected override IOrderedEnumerable<Lot> OrderLots(IEnumerable<Lot> lots) =>
        lots.OrderBy(l => l.PurchaseDate);
}
