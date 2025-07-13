

using PortfolioApp.Domain;
namespace PortfolioApp.Application;
public interface ITradeRepository
{
    IEnumerable<Trade> GetForAsset(Guid assetId);
    void Add(Trade trade);
}
