using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using PortfolioApp.Application;
using PortfolioApp.Application.DTOs;

namespace PortfolioApp.Tests.Api;

// Tests share one app and database; each uses its own ticker. Sell fees are the seeded
// demo rules: 7.00 per trade plus 1% of the trade value.
public class TradingEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public TradingEndpointsTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact(DisplayName = "POST /trades/buy creates the asset and a lot visible in /assets, /assets/id and /lots")]
    public async Task Buy_CreatesAssetAndLot()
    {
        await BuyAsync("BUY1", 100, 10m, "2025-01-01");

        var assets = await HttpAssert.OkAsync<List<AssetDto>>(await _client.GetAsync("/assets"));
        Assert.Contains(new AssetDto("BUY1", 100, 10m), assets);

        var id = await HttpAssert.OkAsync<Guid>(await _client.GetAsync("/assets/id/BUY1"));
        Assert.NotEqual(Guid.Empty, id);

        var lots = await HttpAssert.OkAsync<List<LotDto>>(await _client.GetAsync("/lots"));
        var lot = Assert.Single(lots, l => l.Ticker == "BUY1");
        Assert.Equal((100, 100, 10m), (lot.QtyInitial, lot.QtyRemain, lot.UnitCost));
        Assert.NotEqual(Guid.Empty, lot.Id);
    }

    [Fact(DisplayName = "GET /assets/id/{ticker} returns 404 for an unknown ticker")]
    public async Task AssetId_Unknown_Returns404()
        => await HttpAssert.StatusAsync(HttpStatusCode.NotFound, await _client.GetAsync("/assets/id/NOPE"));

    [Fact(DisplayName = "POST /trades/buy without ticker or assetId returns 400")]
    public async Task Buy_WithoutTicker_Returns400()
    {
        var res = await _client.PostAsJsonAsync("/trades/buy", new { qty = 1, price = 1m, date = "2025-01-01T00:00:00" });
        await HttpAssert.StatusAsync(HttpStatusCode.BadRequest, res);
    }

    [Fact(DisplayName = "POST /trades/sell FIFO returns gross and net profit and records the trade")]
    public async Task Sell_Fifo_ReturnsProfitsAndRecordsTrade()
    {
        await BuyAsync("SELL1", 100, 10m, "2025-01-01");

        var sale = await HttpAssert.OkAsync<SaleResult>(await SellAsync("SELL1", 40, 15m, "FIFO"));

        Assert.Equal(60, sale.RemainingShares);
        Assert.Equal(200m, sale.GrossProfit);           // 40 * (15 - 10)
        Assert.Equal(187m, sale.NetProfit);             // 200 - (7 + 1% of 600)

        var trades = await HttpAssert.OkAsync<List<TradeDto>>(await _client.GetAsync("/trades/all"));
        var sell = Assert.Single(trades, t => t.Ticker == "SELL1" && t.Quantity < 0);
        Assert.Equal((-40, 200m, 187m), (sell.Quantity, sell.ProfitGross, sell.ProfitNet));
        Assert.Contains(trades, t => t.Ticker == "SELL1" && t.Quantity == 100);
    }

    [Fact(DisplayName = "POST /trades/sell with method \"Average\" (the value the UI sends) succeeds")]
    public async Task Sell_Average_Succeeds()
    {
        await BuyAsync("AVG1", 10, 10m, "2025-01-01");
        await BuyAsync("AVG1", 10, 20m, "2025-01-02");

        var sale = await HttpAssert.OkAsync<SaleResult>(await SellAsync("AVG1", 10, 30m, "Average"));

        Assert.Equal(15m, sale.SoldCostPerShare);
        Assert.Equal(150m, sale.GrossProfit);           // 10 * (30 - 15)
        Assert.Equal(140m, sale.NetProfit);             // 150 - (7 + 1% of 300)
    }

    [Fact(DisplayName = "POST /trades/sell with an unknown method returns 400")]
    public async Task Sell_UnknownMethod_Returns400()
    {
        await BuyAsync("BAD1", 10, 10m, "2025-01-01");
        await HttpAssert.StatusAsync(HttpStatusCode.BadRequest, await SellAsync("BAD1", 5, 12m, "AVG"));
    }

    [Fact(DisplayName = "POST /trades/sell more than held returns 409 and changes nothing")]
    public async Task Sell_Oversell_Returns409()
    {
        await BuyAsync("OVER1", 10, 10m, "2025-01-01");

        await HttpAssert.StatusAsync(HttpStatusCode.Conflict, await SellAsync("OVER1", 11, 12m, "FIFO"));

        var lots = await HttpAssert.OkAsync<List<LotDto>>(await _client.GetAsync("/lots"));
        Assert.Equal(10, Assert.Single(lots, l => l.Ticker == "OVER1").QtyRemain);
    }

    [Fact(DisplayName = "OpenAPI document matches the committed portfolio-ui/openapi.json snapshot")]
    public async Task OpenApi_MatchesCommittedSnapshot()
    {
        var live = JsonNode.Parse(await _client.GetStringAsync("/swagger/v1/swagger.json"));
        var committed = JsonNode.Parse(await File.ReadAllTextAsync(FindSnapshot()));

        Assert.True(JsonNode.DeepEquals(live, committed),
            "The API contract changed. Run the backend, then in portfolio-ui: npm run openapi:pull && npm run gen:api");
    }

    private async Task BuyAsync(string ticker, int qty, decimal price, string date)
    {
        var res = await _client.PostAsJsonAsync("/trades/buy", new { ticker, qty, price, date = $"{date}T00:00:00" });
        await HttpAssert.StatusAsync(HttpStatusCode.OK, res);
    }

    private Task<HttpResponseMessage> SellAsync(string ticker, int qty, decimal price, string method)
        => _client.PostAsJsonAsync("/trades/sell", new { ticker, qty, price, method, date = "2025-02-01T00:00:00" });

    private static string FindSnapshot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "portfolio-ui", "openapi.json");
            if (File.Exists(candidate)) return candidate;
        }
        throw new FileNotFoundException("portfolio-ui/openapi.json not found above " + AppContext.BaseDirectory);
    }
}
