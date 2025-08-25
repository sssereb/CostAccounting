
namespace PortfolioApp.Domain;

public class Lot
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid AssetId { get; init; }
    public DateTime PurchaseDate { get; init; }
    public int QtyRemain { get; set; }
    
    public decimal RawUnitCost { get; init; } // + Fees
    public decimal UnitCost { get; init; } // + Fees
    protected Lot() { }
    public Lot(Guid assetId,
        DateTime dt,
        int quantity,
        decimal rawUnitCost,
        decimal unitCostIncludingFees)
    {
        AssetId      = assetId;
        PurchaseDate = dt;
        QtyRemain    = quantity;
        RawUnitCost  = rawUnitCost;
        UnitCost     = unitCostIncludingFees;
    }
}
