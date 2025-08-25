
namespace PortfolioApp.Domain;

public class Asset
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Ticker { get; }
    public Asset(string ticker) => Ticker = ticker.ToUpperInvariant();
}
