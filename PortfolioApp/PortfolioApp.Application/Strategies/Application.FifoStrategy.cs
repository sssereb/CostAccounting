
using PortfolioApp.Domain;
namespace PortfolioApp.Application;

public sealed class FifoStrategy : ICostBasisStrategy
{
    public SaleResult Sell(IList<Lot> lots, int qty, decimal price) =>
        SellHelper.SellFromOrderedLots(lots.OrderBy(l => l.PurchaseDate), qty, price);
}
