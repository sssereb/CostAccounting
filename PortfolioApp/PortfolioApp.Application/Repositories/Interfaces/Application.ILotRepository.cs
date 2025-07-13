
using PortfolioApp.Domain;

namespace PortfolioApp.Application.Repositories.Interfaces;
public interface ILotRepository
{
    IEnumerable<Lot> GetForAsset(Guid assetId);
    void Save(Lot lot);

    IEnumerable<Lot> GetAll();
}
