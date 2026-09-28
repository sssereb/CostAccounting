// PortfolioApp.Tests/Application/TradeServiceTests.cs
using System.Runtime.Serialization;
using Moq;
using PortfolioApp.Application;
using PortfolioApp.Application.Fees;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Application.Strategies;
using PortfolioApp.Domain;

namespace PortfolioApp.Tests;

public class TradeServiceTests
{
    /* ----------------------------------------------------------------
     * BUY: 
     * --------------------------------------------------------------*/
    [Fact(DisplayName = "Buy: Lot & Trade saved with proper fields")]
    public async Task Buy_SavesLotAndTrade()
    {
        var assetId = Guid.NewGuid();
        const int qty = 100;
        const decimal price = 50m;
        var date = new DateTime(2025, 7, 20);

        var assets = new Mock<IAssetRepository>();
        
        var dummyAsset = (Asset)FormatterServices.GetUninitializedObject(typeof(Asset));
        
        assets.Setup(r => r.GetAsync(assetId, It.IsAny<CancellationToken>()))
              .ReturnsAsync(dummyAsset);

        Lot? storedLot = null;
        var lotsRepo = new Mock<ILotRepository>();
        lotsRepo.Setup(r => r.SaveAsync(It.IsAny<Lot>(), It.IsAny<CancellationToken>()))
                .Callback<Lot, CancellationToken>((l, _) => storedLot = l)
                .Returns(Task.CompletedTask);

        var tradeRepo = new Mock<ITradeRepository>();

        var feeSvc = new Mock<IFeeService>();
        feeSvc.Setup(f => f.CalcAll(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<FeeDirection>()))
              .Returns(Array.Empty<Fee>());   // no fees

        var service = new TradeService(
            assets.Object,
            tradeRepo.Object,
            lotsRepo.Object,
            Mock.Of<ICostBasisFactory>(),
            feeSvc.Object);

        await service.BuyAsync(assetId, qty, price, date);
        assets.Verify(r => r.GetAsync(assetId, It.IsAny<CancellationToken>()), Times.Once);

        tradeRepo.Verify(r => r.AddAsync(It.Is<Trade>(t =>
                t.AssetId == assetId &&
                t.Quantity == qty &&
                t.Price    == price),
            It.IsAny<CancellationToken>()), Times.Once);

        Assert.NotNull(storedLot);
        Assert.Equal(assetId, storedLot!.AssetId);
        Assert.Equal(qty,  storedLot.QtyRemain);
        Assert.Equal(price,storedLot.UnitCost);   
        Assert.Equal(date, storedLot.PurchaseDate);
    }

    /* ----------------------------------------------------------------
     * BUY: Asset not found
     * --------------------------------------------------------------*/
    [Fact(DisplayName = "Buy: Asset not found → InvalidOperationException")]
    public async Task Buy_Throws_When_AssetMissing()
    {
        var assets = new Mock<IAssetRepository>();
        assets.Setup(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync((Asset?)null);

        var service = new TradeService(
            assets.Object,
            Mock.Of<ITradeRepository>(),
            Mock.Of<ILotRepository>(),
            Mock.Of<ICostBasisFactory>(),
            Mock.Of<IFeeService>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.BuyAsync(Guid.NewGuid(), 1, 1m, DateTime.Today));
    }

    /* ----------------------------------------------------------------
     * SELL: "Sell: Lot & Trade saved with proper fields"
     * --------------------------------------------------------------*/
    [Fact(DisplayName = "Sell: proper NetProfit")]
    public async Task Sell_PersistsEverything()
    {
        var assetId = Guid.NewGuid();
        var lot = new Lot(assetId, DateTime.Today.AddDays(-1), 50, 90m, 90m);

        var lotsRepo = new Mock<ILotRepository>();
        lotsRepo.Setup(r => r.GetForAssetAsync(assetId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Lot> { lot });

        lotsRepo.Setup(r => r.SaveAsync(It.IsAny<Lot>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        var tradeRepo = new Mock<ITradeRepository>();
        tradeRepo.Setup(r => r.AddAsync(It.IsAny<Trade>(), It.IsAny<CancellationToken>()))
                 .Returns(Task.CompletedTask);

        var stub = new SaleResult(
            RemainingShares: 0,
            SoldCostPerShare: 95m,
            RemainingCostPerShare: 0,
            GrossProfit: 250m,
            NetProfit: 0m);

        var strategy = new Mock<ICostBasisStrategy>();
        strategy.Setup(s => s.Sell(It.IsAny<IList<Lot>>(), 50, 100m))
                .Returns(stub);

        var factory = new Mock<ICostBasisFactory>();
        factory.Setup(f => f.Get(CostBasisMethod.FIFO)).Returns(strategy.Object);

        var feeSvc = new Mock<IFeeService>();
        feeSvc.Setup(f => f.CalcAll(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<FeeDirection>()))
              .Returns(Array.Empty<Fee>());   // no fees

        var service = new TradeService(
            Mock.Of<IAssetRepository>(),
            tradeRepo.Object,
            lotsRepo.Object,
            factory.Object,
            feeSvc.Object);

        var res = await service.SellAsync(assetId, 50, 100m, CostBasisMethod.FIFO, DateTime.Today);
        Assert.Equal(stub.GrossProfit, res.NetProfit);  

        lotsRepo.Verify(r => r.SaveAsync(It.IsAny<Lot>(), It.IsAny<CancellationToken>()),
                        Times.AtLeastOnce());
        tradeRepo.Verify(r => r.AddAsync(It.Is<Trade>(t =>
                t.AssetId == assetId && t.Quantity == -50),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
