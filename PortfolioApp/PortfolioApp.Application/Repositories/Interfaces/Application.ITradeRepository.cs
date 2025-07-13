

using PortfolioApp.Domain;

namespace PortfolioApp.Application.Repositories.Interfaces;
public interface ITradeRepository
{
    IEnumerable<Trade> GetForAsset(Guid assetId);
    void Add(Trade trade);
}
