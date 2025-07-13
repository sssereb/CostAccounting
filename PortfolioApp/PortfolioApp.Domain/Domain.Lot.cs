
namespace PortfolioApp.Domain;

public class Lot
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid AssetId { get; init; }
    public DateTime PurchaseDate { get; init; }
    public int QtyRemain { get; set; }
    public decimal UnitCost { get; init; }
    public Lot(Guid assetId, DateTime date, int qty, decimal cost)
    {
        AssetId = assetId;
        PurchaseDate = date;
        QtyRemain = qty;
        UnitCost = cost;
    }
}
