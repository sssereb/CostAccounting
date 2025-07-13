
namespace PortfolioApp.Domain;

public class Asset
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Ticker { get; }
    public Guid? ClientId { get; set; }
    public Guid? IssuerId { get; set; }
    public Asset(string ticker) => Ticker = ticker.ToUpperInvariant();
}
