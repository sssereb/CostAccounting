using PortfolioApp.Domain;
using System.Threading;

namespace PortfolioApp.Application.Repositories.Interfaces;

public interface ILotRepository
{
    Task<IReadOnlyList<Lot>> GetForAssetAsync(Guid assetId, CancellationToken ct = default);
    Task SaveAsync(Lot lot, CancellationToken ct = default);
    Task<IReadOnlyList<Lot>> GetAllAsync(CancellationToken ct = default);
}