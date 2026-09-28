
using PortfolioApp.Domain;

namespace PortfolioApp.Application.Strategies;

public interface ICostBasisStrategy
{
    CostBasisMethod Method { get; }

    /// <summary>Consumes <paramref name="quantity"/> shares from <paramref name="lots"/> in place.</summary>
    /// <param name="sellFees">Total fees of this sale, subtracted from net profit.</param>
    SaleResult Sell(IList<Lot> lots, int quantity, decimal sellPrice, decimal sellFees);
}

