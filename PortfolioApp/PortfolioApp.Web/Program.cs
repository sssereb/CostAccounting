using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using PortfolioApp.Application;
using PortfolioApp.Application.DTOs;
using PortfolioApp.Application.Fees;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Application.Strategies;
using PortfolioApp.Domain;
using PortfolioApp.Infrastructure.EfCore; // <-- AddPortfolioEfCoreSqlite

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
    builder.Services.AddPortfolioEfCoreSqlite(dbPath);
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
});
builder.Services.AddCors(opt =>
    opt.AddPolicy("Frontend", p =>
        p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

// ── JSON enums как строки
builder.Services.ConfigureHttpJsonOptions(opt =>
{
    opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();
app.UseCors("Frontend");
app.UseSwagger();
app.UseSwaggerUI();

/* ── demo-сидирование комиссий и MSFT (один раз) ── */
using (var scope = app.Services.CreateScope())
{
    var sp   = scope.ServiceProvider;
    var repo = sp.GetRequiredService<IAssetRepository>();
    var svc  = sp.GetRequiredService<TradeService>();
    var prov = sp.GetRequiredService<IFeeRuleProvider>();

    if (!prov.GetRules().Any())
    {
        prov.SetRules(new[]
        {
            new FeeRegistration(FeeType.FixedPerTrade, 7m,    FeeDirection.Sell),
            new FeeRegistration(FeeType.Percent,       0.01m, FeeDirection.Sell)
        });
    }

    // var msft = await repo.GetByTickerAsync("MSFT");
    // if (msft is null)
    // {
    //     msft = await repo.CreateAsync("MSFT");
    //     await svc.BuyAsync(msft.Id, 100, 20m, new DateTime(2025, 1, 1));
    //     await svc.BuyAsync(msft.Id, 150, 30m, new DateTime(2025, 2, 1));
    //     await svc.BuyAsync(msft.Id, 120, 10m, new DateTime(2025, 3, 1));
    // }
}

// ───── Endpoints (ВСЕ async) ─────

// Fees
app.MapGet("/fees/get", (IFeeRuleProvider p)
    => Results.Ok(p.GetRules().Select(r => r.ToDto())));

app.MapPost("/fees/save", (IFeeRuleProvider prov, IEnumerable<FeeRuleDto> body) =>
{
    if (body.Any(r => !Enum.IsDefined(typeof(FeeType), r.Type)))
        return Results.BadRequest("Unknown fee type.");
    if (body.Any(r => r.Amount < 0))
        return Results.BadRequest("Amount must be non-negative.");

    prov.SetRules(body.Select(d => d.ToDomain()));
    return Results.NoContent();
});

app.MapDelete("/fees/delete", (IFeeRuleProvider p) =>
{
    p.SetRules(Array.Empty<FeeRegistration>());
    return Results.NoContent();
});

// Assets
app.MapGet("/assets/id/{ticker}", async (string ticker, IAssetRepository repo) =>
{
    var asset = await repo.GetByTickerAsync(ticker);
    return asset is null
        ? Results.NotFound($"Asset with ticker '{ticker}' not found")
        : Results.Ok(asset.Id);
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

    
    return Results.Ok(result);
});

// BUY
app.MapPost("/trades/buy", async (BuyRequestDto dto, IAssetRepository repo, TradeService svc) =>
{
    if (dto.AssetId is null && string.IsNullOrWhiteSpace(dto.Ticker))
        return Results.BadRequest(new { error = "assetId or ticker required" });

    var assetId = dto.AssetId ?? (await IAssetRepository.GetOrCreateAsync(repo, dto.Ticker!)).Id;
    await svc.BuyAsync(assetId, dto.Qty, dto.Price, dto.Date);
    return Results.Ok();
});

// SELL
app.MapPost("/trades/sell", async (SellRequestDto dto, IAssetRepository repo, TradeService svc) =>
{
    Guid assetId;
    if (dto.AssetId is not null)
        assetId = dto.AssetId.Value;
    else if (!string.IsNullOrWhiteSpace(dto.Ticker))
    {
        var a = await repo.GetByTickerAsync(dto.Ticker);
        if (a is null) return Results.NotFound($"Asset '{dto.Ticker}' not found");
        assetId = a.Id;
    }
    else return Results.BadRequest(new { error = "assetId or ticker required" });

    var res = await svc.SellAsync(assetId, dto.Qty, dto.Price, dto.Method, dto.Date);
    return Results.Ok(res);
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
        .Select(t => new LotDto(t.a.Ticker, t.l.PurchaseDate, t.l.QtyInitial, t.l.QtyRemain, t.l.RawUnitCost, t.l.UnitCost));

    return Results.Ok(result);
});

// Trades
app.MapGet("/trades/all", async (ITradeRepository tradeRepo, IAssetRepository assetRepo) =>
{
    var trades   = await tradeRepo.GetAllAsync();
    var assets = await assetRepo.GetAllAsync();

    var result = trades
        .Join(assets, t => t.AssetId, a => a.Id, (t, a) => new { t, a })
        .OrderBy(t => t.a.Ticker)
        .ThenBy(t => t.t.Date) 
        .Select(t => new TradeDto(t.a.Ticker, t.t.Date, t.t.Quantity, t.t.Price, t.t.ProfitGross, t.t.ProfitNet ));

    return Results.Ok(result);
});

app.UseDefaultFiles(); // index.html, и т.д.
app.UseStaticFiles();  // wwwroot
app.Run("http://localhost:5255");
