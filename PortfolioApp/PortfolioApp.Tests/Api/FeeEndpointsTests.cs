using System.Net;
using System.Net.Http.Json;
using PortfolioApp.Application.DTOs;
using PortfolioApp.Domain;

namespace PortfolioApp.Tests.Api;

// Fee rules live in a singleton, so every test gets its own app instance.
public class FeeEndpointsTests
{
    [Fact(DisplayName = "GET /api/fees/get returns the seeded demo rules")]
    public async Task Get_ReturnsSeededRules()
    {
        using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var rules = await HttpAssert.OkAsync<List<FeeRuleDto>>(await client.GetAsync("/api/fees/get"));

        Assert.Equal(2, rules.Count);
        Assert.All(rules, r => Assert.Equal(FeeDirection.Sell, r.Direction));
    }

    [Fact(DisplayName = "POST /api/fees/save with a Percent rule persists it and GET returns it")]
    public async Task Save_PercentRule_IsReturnedByGet()
    {
        using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var save = await client.PostAsJsonAsync("/api/fees/save", new[]
        {
            new { type = "FixedPerTrade", amount = 5m, direction = "Buy" },
            new { type = "Percent", amount = 0.002m, direction = "Sell" }
        });
        await HttpAssert.StatusAsync(HttpStatusCode.NoContent, save);

        var rules = await HttpAssert.OkAsync<List<FeeRuleDto>>(await client.GetAsync("/api/fees/get"));
        Assert.Equal(
            new[] { new FeeRuleDto(FeeType.FixedPerTrade, 5m, FeeDirection.Buy), new FeeRuleDto(FeeType.Percent, 0.002m, FeeDirection.Sell) },
            rules);
    }

    [Theory(DisplayName = "POST /api/fees/save rejects invalid rules with 400 and keeps the old ones")]
    [InlineData("""[{"type":"PercentOfValue","amount":0.01,"direction":"Sell"}]""")]
    [InlineData("""[{"type":"Percent","amount":-1,"direction":"Sell"}]""")]
    [InlineData("""[{"type":"Percent","amount":0.01,"direction":"Sell"},{"type":"Percent","amount":0.01,"direction":"Sell"}]""")]
    public async Task Save_Invalid_Returns400(string body)
    {
        using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var save = await client.PostAsync("/api/fees/save", new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
        await HttpAssert.StatusAsync(HttpStatusCode.BadRequest, save);

        var rules = await HttpAssert.OkAsync<List<FeeRuleDto>>(await client.GetAsync("/api/fees/get"));
        Assert.Equal(2, rules.Count);
    }

    [Fact(DisplayName = "DELETE /api/fees/delete clears all rules")]
    public async Task Delete_ClearsRules()
    {
        using var factory = new ApiFactory();
        var client = factory.CreateClient();

        await HttpAssert.StatusAsync(HttpStatusCode.NoContent, await client.DeleteAsync("/api/fees/delete"));

        var rules = await HttpAssert.OkAsync<List<FeeRuleDto>>(await client.GetAsync("/api/fees/get"));
        Assert.Empty(rules);
    }
}
