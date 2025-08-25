using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PortfolioApp.Domain;
using PortfolioApp.Application.Repositories.Interfaces;

namespace PortfolioApp.Infrastructure.EfCore.Repositories;

public sealed class EfAssetRepository : IAssetRepository
{
    private readonly PortfolioDbContext _db;
    public EfAssetRepository(PortfolioDbContext db) => _db = db;

    public Task<Asset?> GetAsync(Guid id, CancellationToken ct = default) =>
        _db.Assets.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<Asset?> GetByTickerAsync(string ticker, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ticker))
            return Task.FromResult<Asset?>(null);

        var t = ticker.Trim().ToUpperInvariant();
        return _db.Assets.AsNoTracking().FirstOrDefaultAsync(a => a.Ticker == t, ct);
    }

    public async Task<IReadOnlyList<Asset>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Assets.AsNoTracking().OrderBy(a => a.Ticker).ToListAsync(ct);

    public async Task<Asset> CreateAsync(string ticker, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ticker))
            throw new ArgumentException("Ticker must be non-empty.", nameof(ticker));

        var t = ticker.Trim().ToUpperInvariant();

        var existing = await _db.Assets.FirstOrDefaultAsync(a => a.Ticker == t, ct);
        if (existing is not null) return existing;

        var entity = new Asset(t); // если у Asset другой конструктор — подправь эту строку
        _db.Assets.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }
}