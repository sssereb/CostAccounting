
namespace PortfolioApp.Domain;

public class Lot
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid AssetId { get; init; }
    public DateTime PurchaseDate { get; init; }
    public int QtyInitial { get; set; }
    public int QtyRemain { get; set; }
    
    /// <summary>Purchase price per share, without fees.</summary>
    public decimal RawUnitCost { get; private set; }

    /// <summary>Cost per share including buy fees; this is the cost basis.</summary>
    public decimal UnitCost { get; private set; }

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

    /// <summary>Moves the lot into an average-cost pool: its remaining shares take the pooled costs.</summary>
    public void ApplyAverageCost(decimal rawUnitCost, decimal unitCost)
    {
        RawUnitCost = rawUnitCost;
        UnitCost    = unitCost;
    }
}
