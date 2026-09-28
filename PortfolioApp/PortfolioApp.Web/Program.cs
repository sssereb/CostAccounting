using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using PortfolioApp.Application;
using PortfolioApp.Application.DTOs;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Domain;
using PortfolioApp.Infrastructure.EfCore;
using PortfolioApp.Web.OpenApi;

var builder = WebApplication.CreateBuilder(args);

string GetDefaultDbPath()
{
    var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    var appDir = Path.Combine(root, "PortfolioApp");
    Directory.CreateDirectory(appDir);
    return Path.Combine(appDir, "portfolio.db");
}


// ── DI
builder.Services
    .AddPortfolioCore();                 
   
        
var storageEnv = Environment.GetEnvironmentVariable("PORTFOLIO_STORAGE") ?? "Sqlite"; // "Sqlite" | "InMemory"
var useSqlite  = string.Equals(storageEnv, "Sqlite", StringComparison.OrdinalIgnoreCase);
var dbPath  = Environment.GetEnvironmentVariable("DB_PATH") ?? GetDefaultDbPath();

if (useSqlite)
{
    builder.Services.AddPortfolioEfCoreSqlite();
    builder.Services.AddPortfolioRepositoriesEfCore();
}
else
{
    builder.Services.AddPortfolioRepositoriesInMemory();
}

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

// Fees
app.MapGet("/fees/get", (IFeeRuleProvider p) =>
    TypedResults.Ok(p.GetRules().Select(r => r.ToDto()).ToList()));

app.MapPost("/fees/save", Results<NoContent, BadRequest<string>> (IFeeRuleProvider prov, [FromBody] IEnumerable<FeeRuleDto> body) =>
{
    if (body.Any(r => !Enum.IsDefined(typeof(FeeType), r.Type)))
        return TypedResults.BadRequest("Unknown fee type.");
    if (body.Any(r => r.Amount < 0))
        return TypedResults.BadRequest("Amount must be non-negative.");

    try
    {
        prov.SetRules(body.Select(d => d.ToDomain()));
    }
    catch (InvalidOperationException ex)
    {
        return TypedResults.BadRequest(ex.Message);
    }
    return TypedResults.NoContent();
});

app.MapDelete("/fees/delete", (IFeeRuleProvider p) =>
{
    p.SetRules(Array.Empty<FeeRegistration>());
    return TypedResults.NoContent();
});

// Assets
app.MapGet("/assets/id/{ticker}", async Task<Results<Ok<Guid>, NotFound<string>>> (string ticker, IAssetRepository repo) =>
{
    var asset = await repo.GetByTickerAsync(ticker);
    return asset is null
        ? TypedResults.NotFound($"Asset with ticker '{ticker}' not found")
        : TypedResults.Ok(asset.Id);
});

app.MapGet("/assets", async (IAssetRepository assetrepo, ILotRepository lotrepo)
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
app.MapPost("/trades/buy", async Task<Results<Ok, BadRequest<string>>> (BuyRequestDto dto, IAssetRepository repo, TradeService svc) =>
{
    if (dto.AssetId is null && string.IsNullOrWhiteSpace(dto.Ticker))
        return TypedResults.BadRequest("assetId or ticker required");

    var assetId = dto.AssetId ?? (await IAssetRepository.GetOrCreateAsync(repo, dto.Ticker!)).Id;
    await svc.BuyAsync(assetId, dto.Qty, dto.Price, dto.Date);
    return TypedResults.Ok();
});

// SELL
app.MapPost("/trades/sell", async Task<Results<Ok<SaleResult>, NotFound<string>, BadRequest<string>, ProblemHttpResult>> (
    SellRequestDto dto, IAssetRepository repo, TradeService svc) =>
{
    Guid assetId;
    if (dto.AssetId is not null)
        assetId = dto.AssetId.Value;
    else if (!string.IsNullOrWhiteSpace(dto.Ticker))
    {
        var a = await repo.GetByTickerAsync(dto.Ticker);
        if (a is null) return TypedResults.NotFound($"Asset '{dto.Ticker}' not found");
        assetId = a.Id;
    }
    else return TypedResults.BadRequest("assetId or ticker required");

    try
    {
        var res = await svc.SellAsync(assetId, dto.Qty, dto.Price, dto.Method, dto.Date);
        return TypedResults.Ok(res);
    }
    catch (InvalidOperationException ex)
    {
        return TypedResults.Problem(
            title: "Sell rejected",
            detail: ex.Message,
            statusCode: 409);
    }
    catch (Exception ex)
    {
        return TypedResults.Problem(title: "Sell failed", detail: ex.Message, statusCode: 500);
    }
});

// Lots
app.MapGet("/lots", async (ILotRepository lotRepo, IAssetRepository assetRepo) =>
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
app.MapGet("/trades/all", async (ITradeRepository tradeRepo, IAssetRepository assetRepo) =>
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
app.Run("http://localhost:5255");
