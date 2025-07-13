
using PortfolioApp.Domain;

namespace PortfolioApp.Application.Strategies;

public sealed class LifoStrategy : ICostBasisStrategy
{
    public SaleResult Sell(IList<Lot> lots, int qty, decimal price) =>
        SellHelper.SellFromOrderedLots(lots.OrderByDescending(l => l.PurchaseDate), qty, price);
}
