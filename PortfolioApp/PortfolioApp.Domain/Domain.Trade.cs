
namespace PortfolioApp.Domain;

public record Trade(Guid Id, Guid AssetId, DateTime Date, int Quantity, decimal Price);
