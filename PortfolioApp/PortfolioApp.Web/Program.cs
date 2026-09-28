using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using PortfolioApp.Application;
using PortfolioApp.Application.DTOs;
using PortfolioApp.Application.Fees;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Domain;
using PortfolioApp.Infrastructure.EfCore;
using PortfolioApp.Web.Errors;
using PortfolioApp.Web.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ── DI
builder.Services.AddPortfolioCore();

// PORTFOLIO_STORAGE (Sqlite | InMemory) and DB_PATH are read from configuration, environment variables included.
var storage   = builder.Configuration["PORTFOLIO_STORAGE"] ?? "Sqlite";
var useSqlite = string.Equals(storage, "Sqlite", StringComparison.OrdinalIgnoreCase);

if (useSqlite)
{
    builder.Services.AddPortfolioEfCoreSqlite(DbPathResolver.Resolve(builder.Configuration["DB_PATH"]));
    builder.Services.AddPortfolioRepositoriesEfCore();
}
else
{
    builder.Services.AddPortfolioRepositoriesInMemory();
}

// ── Errors: every failure is answered with ProblemDetails
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProblemExceptionHandler>();

// ── Swagger & CORS
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Portfolio API", Version = "v1" });
    c.SupportNonNullableReferenceTypes();
    c.SchemaFilter<RequireNonNullablePropertiesFilter>();
});
builder.Services.AddCors(opt =>
    opt.AddPolicy("Frontend", p =>
        p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

// ── JSON enums as strings (Swashbuckle reads the MVC options, the endpoints use the HTTP ones)
builder.Services.ConfigureHttpJsonOptions(opt =>
    opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(opt =>
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();   // empty error responses (e.g. binding failures) get a ProblemDetails body
app.UseCors("Frontend");
app.UseSwagger();
app.UseSwaggerUI();

/* ── Demo fee rules (seeded once) ── */
using (var scope = app.Services.CreateScope())
{
    var sp   = scope.ServiceProvider;
    var prov = sp.GetRequiredService<IFeeRuleProvider>();

    if (!prov.GetRules().Any())
    {
        prov.SetRules(new[]
        {
            new FeeRegistration(FeeType.FixedPerTrade, 7m,    FeeDirection.Sell),
            new FeeRegistration(FeeType.Percent,       0.01m, FeeDirection.Sell)
        });
    }
}

// ───── Endpoints ─────
// Under /api in every environment, so the UI calls the same paths via the Vite proxy and when served from wwwroot.
var api = app.MapGroup("/api");

// Fees
api.MapGet("/fees/get", (IFeeRuleProvider p) =>
    TypedResults.Ok(p.GetRules().Select(r => r.ToDto()).ToList()));

api.MapPost("/fees/save", Results<NoContent, ProblemHttpResult> (IFeeRuleProvider prov, [FromBody] IEnumerable<FeeRuleDto> body) =>
{
    if (body.Any(r => !Enum.IsDefined(typeof(FeeType), r.Type)))
        return Invalid("Unknown fee type.");
    if (body.Any(r => r.Amount < 0))
        return Invalid("Amount must be non-negative.");

    prov.SetRules(body.Select(d => d.ToDomain()));   // duplicates throw ArgumentException -> 400
    return TypedResults.NoContent();
})
.ProducesProblem(StatusCodes.Status400BadRequest);

api.MapDelete("/fees/delete", (IFeeRuleProvider p) =>
{
    p.SetRules(Array.Empty<FeeRegistration>());
    return TypedResults.NoContent();
});

// Assets
api.MapGet("/assets/id/{ticker}", async Task<Results<Ok<Guid>, ProblemHttpResult>> (string ticker, IAssetRepository repo) =>
{
    var asset = await repo.GetByTickerAsync(ticker);
    return asset is null
        ? NotFound($"Asset with ticker '{ticker}' not found")
        : TypedResults.Ok(asset.Id);
})
.ProducesProblem(StatusCodes.Status404NotFound);

api.MapGet("/assets", async (IAssetRepository assetrepo, ILotRepository lotrepo)
    =>
{
    var assets = await assetrepo.GetAllAsync();
    var lots = await lotrepo.GetAllAsync();

    var result = assets
        .GroupJoin(lots, a => a.Id, l => l.AssetId, (a, lotGroup) => new { a, lotGroup })
        .Select(x =>
        {
            var ordered = x.lotGroup.OrderBy(l => l.PurchaseDate).ToList();
            var total   = ordered.Sum(l => l.QtyRemain);
            var last    = ordered.LastOrDefault();
            return new AssetDto(x.a.Ticker, total, last?.UnitCost ?? 0m);
        })
        .OrderBy(r => r.Ticker)
        .ToList();

    return TypedResults.Ok(result);
});

// BUY
api.MapPost("/trades/buy", async Task<Results<Ok, ProblemHttpResult>> (BuyRequestDto dto, TradeService svc) =>
{
    if (dto.AssetId is { } assetId)
        await svc.BuyAsync(assetId, dto.Qty, dto.Price, dto.Date);
    else if (!string.IsNullOrWhiteSpace(dto.Ticker))
        await svc.BuyByTickerAsync(dto.Ticker, dto.Qty, dto.Price, dto.Date);
    else
        return Invalid("assetId or ticker required");

    return TypedResults.Ok();
})
.ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status404NotFound);

// SELL
api.MapPost("/trades/sell", async Task<Results<Ok<SaleResult>, ProblemHttpResult>> (
    SellRequestDto dto, IAssetRepository repo, TradeService svc) =>
{
    Guid assetId;
    if (dto.AssetId is not null)
        assetId = dto.AssetId.Value;
    else if (!string.IsNullOrWhiteSpace(dto.Ticker))
    {
        var a = await repo.GetByTickerAsync(dto.Ticker);
        if (a is null) return NotFound($"Asset '{dto.Ticker}' not found");
        assetId = a.Id;
    }
    else return Invalid("assetId or ticker required");

    // Oversell and concurrent changes surface as InvalidOperationException -> 409
    return TypedResults.Ok(await svc.SellAsync(assetId, dto.Qty, dto.Price, dto.Method, dto.Date));
})
.ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status404NotFound)
.ProducesProblem(StatusCodes.Status409Conflict);

// Lots
api.MapGet("/lots", async (ILotRepository lotRepo, IAssetRepository assetRepo) =>
{
    var lots   = await lotRepo.GetAllAsync();
    var assets = await assetRepo.GetAllAsync();

    var result = lots
        .Join(assets, l => l.AssetId, a => a.Id, (l, a) => new { l, a })
        .OrderBy(t => t.a.Ticker)
        .ThenBy(t => t.l.PurchaseDate) 
        .Select(t => new LotDto(t.l.Id, t.a.Ticker, t.l.PurchaseDate, t.l.QtyInitial, t.l.QtyRemain, t.l.RawUnitCost, t.l.UnitCost))
        .ToList();

    return TypedResults.Ok(result);
});

// Trades
api.MapGet("/trades/all", async (ITradeRepository tradeRepo, IAssetRepository assetRepo) =>
{
    var trades   = await tradeRepo.GetAllAsync();
    var assets = await assetRepo.GetAllAsync();

    var result = trades
        .Join(assets, t => t.AssetId, a => a.Id, (t, a) => new { t, a })
        .OrderBy(t => t.t.Date)
        .ThenBy(t => t.a.Ticker) 
        .Select(t => new TradeDto(t.t.Id, t.a.Ticker, t.t.Date, t.t.Quantity, t.t.Price, t.t.ProfitGross, t.t.ProfitNet))
        .ToList();

    return TypedResults.Ok(result);
});

app.UseDefaultFiles(); // index.html etc.
app.UseStaticFiles();  // wwwroot
app.Run();

static ProblemHttpResult Invalid(string detail) =>
    TypedResults.Problem(detail, statusCode: StatusCodes.Status400BadRequest, title: "Invalid request");

static ProblemHttpResult NotFound(string detail) =>
    TypedResults.Problem(detail, statusCode: StatusCodes.Status404NotFound, title: "Not found");

// Lets WebApplicationFactory<Program> reference the entry point from the test project.
public partial class Program;
