using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PortfolioApp.Application.Persistence;

namespace PortfolioApp.Infrastructure.EfCore;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly PortfolioDbContext _db;

    public EfUnitOfWork(PortfolioDbContext db) => _db = db;

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> work, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            await work(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await RollbackAsync(tx);
            throw new ConcurrencyConflictException(
                "The data was changed by another operation. Reload and try again.", ex);
        }
        catch
        {
            await RollbackAsync(tx);
            throw;
        }
    }

    private async Task RollbackAsync(IDbContextTransaction tx)
    {
        await tx.RollbackAsync(CancellationToken.None);
        _db.ChangeTracker.Clear();
    }
}
