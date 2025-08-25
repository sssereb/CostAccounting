using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Domain;

namespace PortfolioApp.Infrastructure.EfCore.Repositories;

public sealed class EfTradeRepository : ITradeRepository
{
    private readonly PortfolioDbContext _db;
    public EfTradeRepository(PortfolioDbContext db) => _db = db;

    public async Task AddAsync(Trade trade, CancellationToken ct = default)
    {
        _db.Trades.Add(trade);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Trade>> GetForAssetAsync(Guid assetId, CancellationToken ct = default) =>
        await _db.Trades
            .AsNoTracking()
            .Where(t => t.AssetId == assetId)
            .OrderBy(t => t.Date)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Trade>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Trades
            .AsNoTracking()
            .OrderBy(t => t.AssetId).ThenBy(t => t.Date)
            .ToListAsync(ct);
}