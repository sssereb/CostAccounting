using PortfolioApp.Application.Strategies;   // FifoStrategy, LifoStrategy, AvgStrategy, ICostBasisStrategy
using PortfolioApp.Domain;            // Lot, SaleResult

namespace PortfolioApp.Tests;

public class CostBasisStrategyTests
{
    // ----------------------- positive -----------------------

    // qty1, cost1, qty2, cost2, sellQty, expectedSoldCostPerShare
    public static IEnumerable<object[]> FifoData => new[]
    {
        new object[] { 10, 100m,  0,   0m,  5 , 100.0000m },
        new object[] { 10, 100m, 20, 120m, 15 , 106.6667m },
        new object[] { 10, 100m, 20, 120m, 30 , 113.3333m }
    };

    public static IEnumerable<object[]> LifoData => new[]
    {
        new object[] { 10, 100m,  0,   0m,  5 , 100.0000m },
        new object[] { 10, 100m, 20, 120m, 15 , 120.0000m },
        new object[] { 10, 100m, 20, 120m, 25 , 116.0000m }
    };

    public static IEnumerable<object[]> AvgData => new[]
    {
        new object[] { 10, 100m,  0,   0m,  5 , 100.0000m },
        new object[] { 10, 100m, 20, 120m, 15 , 113.3333m },
        new object[] { 10, 100m, 20, 120m, 30 , 113.3333m }
    };

    // -------- FIFO --------
    [Theory(DisplayName = "FIFO: SoldCostPerShare")]
    [MemberData(nameof(FifoData))]
    public void Fifo_Correct(decimal q1, decimal c1, decimal q2, decimal c2, int sellQty, decimal expected)
        => PositiveCase(new FifoStrategy(), q1, c1, q2, c2, sellQty, expected);

    // -------- LIFO --------
    [Theory(DisplayName = "LIFO: SoldCostPerShare")]
    [MemberData(nameof(LifoData))]
    public void Lifo_Correct(decimal q1, decimal c1, decimal q2, decimal c2, int sellQty, decimal expected)
        => PositiveCase(new LifoStrategy(), q1, c1, q2, c2, sellQty, expected);

    // -------- AVG ---------
    [Theory(DisplayName = "AVG: SoldCostPerShare")]
    [MemberData(nameof(AvgData))]
    public void Avg_Correct(decimal q1, decimal c1, decimal q2, decimal c2, int sellQty, decimal expected)
        => PositiveCase(new AverageCostStrategy(), q1, c1, q2, c2, sellQty, expected);

    // ----------------------- negative ----------------------

    [Fact(DisplayName = "FIFO: oversell ⇒ Exception + QtyRemain unchanged")]
    public void Fifo_Oversell()
        => OversellCase(new FifoStrategy());

    [Fact(DisplayName = "LIFO: oversell ⇒ Exception + QtyRemain unchanged")]
    public void Lifo_Oversell()
        => OversellCase(new LifoStrategy());

    [Fact(DisplayName = "AVG: oversell ⇒ Exception + QtyRemain unchanged")]
    public void Avg_Oversell()
        => OversellCase(new AverageCostStrategy());

    // ====================== helpers ======================

    private static void PositiveCase(
        ICostBasisStrategy strategy,
        decimal q1, decimal c1,
        decimal q2, decimal c2,
        int sellQty,
        decimal expSoldCost)
    {
        var lots = BuildLots((int)q1, c1, (int)q2, c2);
        var startQty = lots.Sum(l => l.QtyRemain);

        var res = strategy.Sell(lots, sellQty, sellPrice: 0m);

        Assert.Equal(expSoldCost, res.SoldCostPerShare, 4);          
        Assert.Equal(startQty - sellQty, lots.Sum(l => l.QtyRemain));
    }

    private static void OversellCase(ICostBasisStrategy strategy)
    {
        var lots = BuildLots(10, 100m, 15, 120m);
        var snapshot = lots.ToDictionary(l => l.Id, l => l.QtyRemain);

        var ex = Record.Exception(() => strategy.Sell(lots, 40, 0m));
        Assert.IsType<InvalidOperationException>(ex);

        foreach (var lot in lots)
            Assert.Equal(snapshot[lot.Id], lot.QtyRemain);
    }

    private static List<Lot> BuildLots(int qty1, decimal cost1, int qty2, decimal cost2)
    {
        var today = DateTime.Today;

        var list = new List<Lot>
        {
            new(Guid.NewGuid(), today.AddDays(-2), qty1, cost1, cost1)
        };

        if (qty2 > 0)
        {
            list.Add(new(Guid.NewGuid(), today.AddDays(-1), qty2, cost2, cost2));
        }

        return list;
    }

}
