
using PortfolioApp.Domain;

namespace PortfolioApp.Application.Strategies;

public interface ICostBasisStrategy
{
    CostBasisMethod Method { get; }

    /// <summary>Consumes <paramref name="qty"/> shares from <paramref name="lots"/> in place.</summary>
    /// <param name="sellFees">Total fees of this sale, subtracted from net profit.</param>
    SaleResult Sell(IList<Lot> lots, int qty, decimal sellPrice, decimal sellFees);
}

