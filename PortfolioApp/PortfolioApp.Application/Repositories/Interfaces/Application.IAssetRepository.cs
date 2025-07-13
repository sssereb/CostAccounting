
using PortfolioApp.Domain;

namespace PortfolioApp.Application.Repositories.Interfaces;

public interface IAssetRepository
{
    Asset? Get(Guid id);
    Asset? GetByTicker(string ticker);
    Asset Create(string ticker);
    IEnumerable<Asset> GetAll();
    public static Asset GetOrCreate(IAssetRepository repo, string ticker)
        => repo.GetByTicker(ticker) ?? repo.Create(ticker);

}
