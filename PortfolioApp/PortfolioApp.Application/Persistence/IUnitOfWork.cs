namespace PortfolioApp.Application.Persistence;

public interface IUnitOfWork
{
    /// <summary>
    /// Runs <paramref name="work"/> so that all of its writes are committed together or not at all.
    /// Throws <see cref="ConcurrencyConflictException"/> when data read earlier was changed by someone else.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> work, CancellationToken ct = default);
}
