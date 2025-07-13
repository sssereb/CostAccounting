
using PortfolioApp.Domain;

namespace PortfolioApp.Application;

public interface IAssetRepository
{
    Asset? Get(Guid id);
    Asset? GetByTicker(string ticker);
    Asset Create(string ticker);
    IEnumerable<Asset> GetAll();
}
