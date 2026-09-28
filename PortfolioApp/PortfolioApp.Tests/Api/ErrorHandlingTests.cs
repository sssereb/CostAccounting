using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Domain;

namespace PortfolioApp.Tests.Api;

public class ErrorHandlingTests
{
    [Fact(DisplayName = "Unexpected exception ⇒ 500 ProblemDetails with a generic message, internals not leaked")]
    public async Task UnexpectedException_Returns500WithoutDetails()
    {
        var trades = new Mock<ITradeRepository>();
        trades.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
              .ThrowsAsync(new Exception("connection string Password=secret"));

        using var factory = new ApiFactory();
        var client = factory
            .WithWebHostBuilder(b => b.ConfigureTestServices(s => s.AddScoped(_ => trades.Object)))
            .CreateClient();

        var res = await client.GetAsync("/trades/all");

        await HttpAssert.StatusAsync(HttpStatusCode.InternalServerError, res);
        Assert.Equal("application/problem+json", res.Content.Headers.ContentType?.MediaType);
        var body = await res.Content.ReadAsStringAsync();
        Assert.DoesNotContain("secret", body);
        Assert.Equal("An unexpected error occurred.", (await ReadProblem(res)).Detail);
    }

    [Fact(DisplayName = "Rule violation ⇒ 409 ProblemDetails carrying the domain message")]
    public async Task Oversell_Returns409Problem()
    {
        using var factory = new ApiFactory();
        var client = factory.CreateClient();
        await client.PostAsJsonAsync("/trades/buy", new { ticker = "ERR1", qty = 1, price = 1m, date = "2025-01-01T00:00:00" });

        var res = await client.PostAsJsonAsync("/trades/sell",
            new { ticker = "ERR1", qty = 2, price = 1m, method = "FIFO", date = "2025-01-02T00:00:00" });

        await HttpAssert.StatusAsync(HttpStatusCode.Conflict, res);
        var problem = await ReadProblem(res);
        Assert.Equal(409, problem.Status);
        Assert.Contains("Not enough shares", problem.Detail);
    }

    [Fact(DisplayName = "Invalid argument ⇒ 400 ProblemDetails")]
    public async Task InvalidQuantity_Returns400Problem()
    {
        using var factory = new ApiFactory();
        var client = factory.CreateClient();

        var res = await client.PostAsJsonAsync("/trades/buy", new { ticker = "ERR2", qty = 0, price = 1m, date = "2025-01-01T00:00:00" });

        await HttpAssert.StatusAsync(HttpStatusCode.BadRequest, res);
        Assert.Equal(400, (await ReadProblem(res)).Status);
    }

    private static async Task<ProblemDetails> ReadProblem(HttpResponseMessage res)
        => (await res.Content.ReadFromJsonAsync<ProblemDetails>(ApiFactory.Json))!;
}
