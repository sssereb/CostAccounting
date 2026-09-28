using Microsoft.EntityFrameworkCore;
using Moq;
using PortfolioApp.Application;
using PortfolioApp.Application.Fees;
using PortfolioApp.Application.Persistence;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Application.Strategies;
using PortfolioApp.Domain;
using PortfolioApp.Infrastructure.EfCore;
using PortfolioApp.Infrastructure.EfCore.Repositories;
using PortfolioApp.Tests.Support;

namespace PortfolioApp.Tests;

public class SellAtomicityTests
{
    private static readonly DateTime BuyDate = new(2025, 1, 1);
    private static readonly DateTime SellDate = new(2025, 2, 1);

    [Fact(DisplayName = "Sell: a failed trade insert rolls back the lot changes")]
    public async Task Sell_WhenTradeInsertFails_DoesNotPersistLotChanges()
    {
        using var database = new SqliteTestDatabase();
        var assetId = await SeedLotAsync(database, qty: 100);

        var failingTrades = new Mock<ITradeRepository>();
        failingTrades.Setup(r => r.AddAsync(It.IsAny<Trade>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new IOException("disk full"));

        await using (var db = database.CreateContext())
        {
            var service = CreateService(db, trades: failingTrades.Object);
            await Assert.ThrowsAsync<IOException>(() =>
                service.SellAsync(assetId, 40, 12m, CostBasisMethod.FIFO, SellDate));
        }

        await using var verify = database.CreateContext();
        var lot = Assert.Single(await verify.Lots.ToListAsync());
        Assert.Equal(100, lot.QtyRemain);
        Assert.Equal(0, lot.Version);
        Assert.Single(await verify.Trades.ToListAsync());
    }

    [Fact(DisplayName = "Sell: of two concurrent sales of the same lots, the second conflicts and writes nothing")]
    public async Task ConcurrentSells_SecondFailsWithConflict()
    {
        using var database = new SqliteTestDatabase();
        var assetId = await SeedLotAsync(database, qty: 100);

        await using var dbA = database.CreateContext();
        await using var dbB = database.CreateContext();
        var pausedLots = new PausingLotRepository(new EfLotRepository(dbA));
        var sellerA = CreateService(dbA, lots: pausedLots);
        var sellerB = CreateService(dbB);

        var saleA = sellerA.SellAsync(assetId, 60, 12m, CostBasisMethod.FIFO, SellDate);
        await pausedLots.LotsRead;
        await sellerB.SellAsync(assetId, 60, 12m, CostBasisMethod.FIFO, SellDate);
        pausedLots.Resume();

        var conflict = await Assert.ThrowsAsync<ConcurrencyConflictException>(() => saleA);
        Assert.IsAssignableFrom<InvalidOperationException>(conflict);

        await using var verify = database.CreateContext();
        var lot = Assert.Single(await verify.Lots.ToListAsync());
        Assert.Equal(40, lot.QtyRemain);
        Assert.Equal(1, lot.Version);
        Assert.Equal(2, await verify.Trades.CountAsync());
    }

    private static async Task<Guid> SeedLotAsync(SqliteTestDatabase database, int qty)
    {
        await using var db = database.CreateContext();
        var asset = await new EfAssetRepository(db).CreateAsync("MSFT");
        await CreateService(db).BuyAsync(asset.Id, qty, 10m, BuyDate);
        return asset.Id;
    }

    private static TradeService CreateService(
        PortfolioDbContext db, ILotRepository? lots = null, ITradeRepository? trades = null)
    {
        var noFees = new Mock<IFeeService>();
        noFees.Setup(f => f.CalcAll(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<FeeDirection>()))
              .Returns(Array.Empty<Fee>());

        var strategies = new ICostBasisStrategy[] { new FifoStrategy(), new LifoStrategy(), new AverageCostStrategy() };

        return new TradeService(
            new EfAssetRepository(db),
            trades ?? new EfTradeRepository(db),
            lots ?? new EfLotRepository(db),
            new CostBasisFactory(strategies),
            noFees.Object,
            new EfUnitOfWork(db));
    }
}
