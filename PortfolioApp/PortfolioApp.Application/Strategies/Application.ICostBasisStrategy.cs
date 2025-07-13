
using PortfolioApp.Domain;

namespace PortfolioApp.Application.Strategies;

public interface ICostBasisStrategy
{
    SaleResult Sell(IList<Lot> lots, int quantity, decimal sellPrice);
}

