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

    [Fact(DisplayName = "FIFO: oversell throws and leaves QtyRemain unchanged")]
    public void Fifo_Oversell()
        => OversellCase(new FifoStrategy());

    [Fact(DisplayName = "LIFO: oversell throws and leaves QtyRemain unchanged")]
    public void Lifo_Oversell()
        => OversellCase(new LifoStrategy());

    [Fact(DisplayName = "AVG: oversell throws and leaves QtyRemain unchanged")]
    public void Avg_Oversell()
        => OversellCase(new AverageCostStrategy());

    // ------------------- average-cost pool -------------------

    [Fact(DisplayName = "AVG then FIFO: remaining shares keep the averaged cost")]
    public void AverageSale_ThenFifoSale_UsesAveragedCost()
    {
        var lots = BuildLots(10, 100m, 20, 120m);

        var avg = new AverageCostStrategy().Sell(lots, 15, 150m, 0m);
        var fifo = new FifoStrategy().Sell(lots, 5, 150m, 0m);

        Assert.Equal(113.3333m, avg.SoldCostPerShare, 4);
        Assert.Equal(113.3333m, fifo.SoldCostPerShare, 4);
        Assert.Equal(113.3333m, fifo.RemainingCostPerShare, 4);
        Assert.All(lots.Where(l => l.QtyRemain > 0), l => Assert.Equal(113.3333m, l.UnitCost, 4));
    }

    // ------------------- profit definitions -------------------

    public static IEnumerable<object[]> AllStrategies => new[]
    {
        new object[] { new FifoStrategy() },
        new object[] { new LifoStrategy() },
        new object[] { new AverageCostStrategy() }
    };

    // 10 shares bought at 100 with a 10.00 buy fee (unit cost 101), sold at 120 with a 7.00 sell fee.
    [Theory(DisplayName = "Profit: gross ignores fees, net subtracts buy and sell fees")]
    [MemberData(nameof(AllStrategies))]
    public void Profit_WithBuyAndSellFees(ICostBasisStrategy strategy)
    {
        var lots = new List<Lot> { new(Guid.NewGuid(), DateTime.Today, 10, rawUnitCost: 100m, unitCostIncludingFees: 101m) };

        var res = strategy.Sell(lots, 10, 120m, sellFees: 7m);

        Assert.Equal(200m, res.GrossProfit);
        Assert.Equal(183m, res.NetProfit);
    }

    [Fact(DisplayName = "Profit: AVG over two lots with buy and sell fees")]
    public void Profit_Average_TwoLotsWithFees()
    {
        var lots = new List<Lot>
        {
            new(Guid.NewGuid(), DateTime.Today.AddDays(-2), 10, rawUnitCost: 100m, unitCostIncludingFees: 101m),
            new(Guid.NewGuid(), DateTime.Today.AddDays(-1), 10, rawUnitCost: 120m, unitCostIncludingFees: 121m)
        };

        var res = new AverageCostStrategy().Sell(lots, 10, 130m, sellFees: 5m);

        Assert.Equal(200m, res.GrossProfit);   // 10 * (130 - 110)
        Assert.Equal(185m, res.NetProfit);     // 10 * (130 - 111) - 5
    }

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

        var res = strategy.Sell(lots, sellQty, sellPrice: 0m, sellFees: 0m);

        Assert.Equal(expSoldCost, res.SoldCostPerShare, 4);          
        Assert.Equal(startQty - sellQty, lots.Sum(l => l.QtyRemain));
    }

    private static void OversellCase(ICostBasisStrategy strategy)
    {
        var lots = BuildLots(10, 100m, 15, 120m);
        var snapshot = lots.ToDictionary(l => l.Id, l => l.QtyRemain);

        var ex = Record.Exception(() => strategy.Sell(lots, 40, 0m, 0m));
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
