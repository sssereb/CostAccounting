// PortfolioApp.Console.Tests/Application/TradeServiceTests.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Moq;
using PortfolioApp.Application;
using PortfolioApp.Application.Fees;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Application.Strategies;
using PortfolioApp.Domain;
using Xunit;

namespace PortfolioApp.Tests;

public class TradeServiceTests
{
    /* ----------------------------------------------------------------
     * Вспомогательная утилита: “нулевой” FeeComposite (комиссии = 0)
     * --------------------------------------------------------------*/
    private static FeeComposite ZeroFeeComposite() => new(new[]
    {
        new FeeRegistration(new FixedPerTrade(0m), FeeDirection.Buy),
        new FeeRegistration(new FixedPerTrade(0m), FeeDirection.Sell)
    });

    /* ----------------------------------------------------------------
     * BUY: успешная покупка
     *  – ищет актив;
     *  – сохраняет лот и сделку;
     *  – UnitCost = price (комиссий нет).
     * --------------------------------------------------------------*/
    [Fact(DisplayName = "Buy: сохраняет Lot и Trade с корректными полями")]
    public void Buy_SavesLotAndTrade()
    {
        // исходные данные
        var assetId = Guid.NewGuid();
        const int qty = 100;
        const decimal price = 50m;
        var date = new DateTime(2025, 7, 20);

        // IAssetRepository → возвращаем «пустой» Asset
        var assets = new Mock<IAssetRepository>();
        var dummyAsset = (Asset)FormatterServices.GetUninitializedObject(typeof(Asset));
        assets.Setup(r => r.Get(assetId)).Returns(dummyAsset);

        // перехватываем сохранённый Lot
        Lot? storedLot = null;
        var lotsRepo = new Mock<ILotRepository>();
        lotsRepo.Setup(r => r.Save(It.IsAny<Lot>()))
                .Callback<Lot>(l => storedLot = l);

        var tradeRepo = new Mock<ITradeRepository>();

        var service = new TradeService(
            assets.Object,
            tradeRepo.Object,
            lotsRepo.Object,
            Mock.Of<ICostBasisFactory>(),
            ZeroFeeComposite());

        // act
        service.Buy(assetId, qty, price, date);

        // assert
        assets.Verify(r => r.Get(assetId), Times.Once);
        tradeRepo.Verify(r => r.Add(It.Is<Trade>(t =>
            t.AssetId == assetId &&
            t.Quantity     == qty &&
            t.Price   == price)), Times.Once);

        Assert.NotNull(storedLot);
        Assert.Equal(assetId, storedLot!.AssetId);
        Assert.Equal(qty,      storedLot.QtyRemain);
        Assert.Equal(price,    storedLot.UnitCost);      // комиссии = 0
        Assert.Equal(date,     storedLot.PurchaseDate);
    }

    /* ----------------------------------------------------------------
     * BUY: актив не найден → InvalidOperationException
     * --------------------------------------------------------------*/
    [Fact(DisplayName = "Buy: Asset отсутствует — бросает InvalidOperationException")]
    public void Buy_Throws_When_AssetMissing()
    {
        var assets = new Mock<IAssetRepository>();
        assets.Setup(r => r.Get(It.IsAny<Guid>())).Returns((Asset?)null);

        var service = new TradeService(
            assets.Object,
            Mock.Of<ITradeRepository>(),
            Mock.Of<ILotRepository>(),
            Mock.Of<ICostBasisFactory>(),
            ZeroFeeComposite());

        Assert.Throws<InvalidOperationException>(() =>
            service.Buy(Guid.NewGuid(), 1, 1m, DateTime.Today));
    }

    /* ----------------------------------------------------------------
     * SELL: проверяем orchestration
     *  – вызывает стратегию;
     *  – сохраняет изменённые лоты;
     *  – добавляет сделку;
     *  – NetProfit = GrossProfit (fee = 0).
     * --------------------------------------------------------------*/
    [Fact(DisplayName = "Sell: корректно возвращает NetProfit и сохраняет изменения")]
    public void Sell_PersistsEverything()
    {
        var assetId = Guid.NewGuid();
        var lot = new Lot(assetId, DateTime.Today.AddDays(-1), 50, 90m, 90m);

        var lotsRepo = new Mock<ILotRepository>();
        lotsRepo.Setup(r => r.GetForAsset(assetId))
                .Returns(new List<Lot> { lot });

        var tradeRepo = new Mock<ITradeRepository>();

        // мок-стратегия отдаёт фиксированный результат
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

        var service = new TradeService(
            Mock.Of<IAssetRepository>(),
            tradeRepo.Object,
            lotsRepo.Object,
            factory.Object,
            ZeroFeeComposite());

        // act
        var res = service.Sell(assetId, 50, 100m, CostBasisMethod.FIFO, DateTime.Today);

        // assert
        Assert.Equal(stub.GrossProfit, res.NetProfit);      // fee = 0

        lotsRepo.Verify(r => r.Save(It.IsAny<Lot>()), Times.AtLeastOnce());
        tradeRepo.Verify(r => r.Add(It.Is<Trade>(t =>
            t.AssetId == assetId && t.Quantity == -50)), Times.Once);
    }
}
