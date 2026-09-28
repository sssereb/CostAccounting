using PortfolioApp.Application.Persistence;

namespace PortfolioApp.Application.Repositories.InMemory;

/// <summary>The in-memory store has no transactions; writes apply immediately.</summary>
public sealed class NoOpUnitOfWork : IUnitOfWork
{
    public Task ExecuteInTransactionAsync(Func<CancellationToken, Task> work, CancellationToken ct = default) => work(ct);
}
