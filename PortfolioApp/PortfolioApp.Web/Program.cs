using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using PortfolioApp.Application;
using PortfolioApp.Application.DTOs;
using PortfolioApp.Application.Repositories.InMemory;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Application.Strategies;
using PortfolioApp.Domain;

var builder = WebApplication.CreateBuilder(args);

// ───── DI: регистрируем всё, что было в консоли ─────
builder.Services.AddPortfolioCore();

// ───── Swagger & CORS ─────
builder.Services.AddEndpointsApiExplorer();

 builder.Services.AddSwaggerGen(c =>
 {
     c.SwaggerDoc("v1", new OpenApiInfo { Title = "Portfolio API", Version = "v1" });
 });
 builder.Services.AddCors(opt =>
     opt.AddPolicy("Frontend", p =>
         p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));
 
builder.Services.ConfigureHttpJsonOptions(opt =>
 {
     opt.SerializerOptions.Converters.Add(
         new JsonStringEnumConverter(null));
 });

 var app = builder.Build();
 app.UseCors("Frontend");
 app.UseSwagger();
 app.UseSwaggerUI();

/* ── demo-сидирование ── */
 using (var scope = app.Services.CreateScope())
 {
     var repo = scope.ServiceProvider.GetRequiredService<IAssetRepository>();
     var svc  = scope.ServiceProvider.GetRequiredService<TradeService>();

     if (repo.GetByTicker("MSFT") is null)
     {
         var asset = repo.Create("MSFT");
         svc.Buy(asset.Id, 100, 20m, new DateTime(2025, 1, 1));
         svc.Buy(asset.Id, 150, 30m, new DateTime(2025, 2, 1));
         svc.Buy(asset.Id, 120, 10m, new DateTime(2025, 3, 1));
     }
 }
 
 // ───── Endpoints ─────
 app.MapGet("/assets/id/{ticker}", (string ticker, IAssetRepository repo) =>
 {
     var asset = repo.GetByTicker(ticker);      // может вернуться null

     if (asset is null)
         return Results.NotFound($"Asset with ticker '{ticker}' not found");

     return Results.Ok(asset.Id);
 });

 app.MapGet("/assets", (IAssetRepository repo) =>
 {
     return repo.GetAll();
 });

 // BUY
 app.MapPost("/trades/buy", (BuyRequestDto dto,
     IAssetRepository repo,
     TradeService svc) =>
 {
     Guid assetId = dto.AssetId ?? repo.GetOrCreate(dto.Ticker!)
         .Id;                 // бросит NRE, если null
     svc.Buy(assetId, dto.Qty, dto.Price, dto.Date);
     return Results.Ok();
 });

// SELL
 app.MapPost("/trades/sell", (SellRequestDto dto,
     IAssetRepository repo,
     TradeService svc) =>
 {
     Guid assetId;
     if (dto.AssetId is not null)
         assetId = dto.AssetId.Value;
     else if (dto.Ticker is not null && repo.GetByTicker(dto.Ticker) is { } a)
         assetId = a.Id;
     else
         return Results.BadRequest(new { error = "assetId or ticker required" });

     var res = svc.Sell(assetId, dto.Qty, dto.Price, dto.Method, dto.Date);
     return Results.Ok(res);
 });

 app.MapGet("/lots", 
     (ILotRepository lotRepo, IAssetRepository assetRepo) =>
     
     { var lots = lotRepo.GetAll()
             .Join(assetRepo.GetAll(), l => l.AssetId, a => a.Id, (l, a) => new { l, a })
             .OrderBy(@t => t.a.Ticker)
             .ThenBy(@t => t.l.PurchaseDate)
             .Select(@t => new LotDto(t.a.Ticker, t.l.PurchaseDate, t.l.QtyRemain, t.l.RawUnitCost, t.l.UnitCost)); 
         return Results.Ok(lots); 
     });

 app.Run("http://localhost:5255");

