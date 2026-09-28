namespace PortfolioApp.Application.Persistence;

public sealed class ConcurrencyConflictException : InvalidOperationException
{
    public ConcurrencyConflictException(string message, Exception? inner = null) : base(message, inner) { }
}
