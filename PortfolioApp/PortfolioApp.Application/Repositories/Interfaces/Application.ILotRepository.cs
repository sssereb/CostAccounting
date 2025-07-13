
using PortfolioApp.Domain;
namespace PortfolioApp.Application;
public interface ILotRepository
{
    IEnumerable<Lot> GetForAsset(Guid assetId);
    void Save(Lot lot);
}
