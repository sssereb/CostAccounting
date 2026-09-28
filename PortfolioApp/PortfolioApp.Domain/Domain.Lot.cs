
namespace PortfolioApp.Domain;

public class Lot
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid AssetId { get; init; }
    public DateTime PurchaseDate { get; init; }
    public int QtyInitial { get; set; }
    public int QtyRemain { get; set; }
    
    public decimal RawUnitCost { get; init; } 
    public decimal UnitCost { get; init; } 

    /// <summary>Optimistic concurrency token, incremented on every update.</summary>
    public int Version { get; private set; }
    protected Lot() { }
    public Lot(Guid assetId,
        DateTime dt,
        int quantity,
        decimal rawUnitCost,
        decimal unitCostIncludingFees)
    {
        AssetId      = assetId;
        PurchaseDate = dt;
        QtyInitial   = quantity;
        QtyRemain    = quantity;
        RawUnitCost  = rawUnitCost;
        UnitCost     = unitCostIncludingFees;
    }
}
