using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Domain;

namespace PortfolioApp.Infrastructure.EfCore.Repositories;

public sealed class EfLotRepository : ILotRepository
{
    private readonly PortfolioDbContext _db;
    public EfLotRepository(PortfolioDbContext db) => _db = db;

    public async Task<IReadOnlyList<Lot>> GetForAssetAsync(Guid assetId, CancellationToken ct = default) =>
        await _db.Lots
            .AsNoTracking()
            .Where(l => l.AssetId == assetId)
            .OrderBy(l => l.PurchaseDate) // удобно для FIFO
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Lot>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Lots
            .AsNoTracking()
            .OrderBy(l => l.AssetId).ThenBy(l => l.PurchaseDate)
            .ToListAsync(ct);

    public async Task SaveAsync(Lot lot, CancellationToken ct = default)
    {
        // upsert по Id
        var exists = await _db.Lots.AsNoTracking().AnyAsync(x => x.Id == lot.Id, ct);
        if (exists) _db.Lots.Update(lot);
        else        _db.Lots.Add(lot);

        await _db.SaveChangesAsync(ct);
    }
}