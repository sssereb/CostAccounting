using PortfolioApp.Domain;

namespace PortfolioApp.Application.Repositories.Interfaces;

public interface ITradeRepository
{
    Task AddAsync(Trade trade, CancellationToken ct = default);
    Task<IReadOnlyList<Trade>> GetAllAsync(CancellationToken ct = default);
}