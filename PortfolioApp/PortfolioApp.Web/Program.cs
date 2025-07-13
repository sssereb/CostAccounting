using Microsoft.OpenApi;
using PortfolioApp.Application;

var builder = WebApplication.CreateBuilder(args);

// ───── DI: регистрируем всё, что было в консоли ─────
builder.Services.AddSingleton<IAssetRepository, InMemoryAssetRepository>();
builder.Services.AddSingleton<ILotRepository, InMemoryLotRepository>();
builder.Services.AddSingleton<ITradeRepository, InMemoryTradeRepository>();
builder.Services.AddSingleton<FifoStrategy>();
builder.Services.AddSingleton<LifoStrategy>();
builder.Services.AddSingleton<AverageCostStrategy>();
builder.Services.AddSingleton<ICostBasisFactory, CostBasisFactory>();
builder.Services.AddSingleton<TradeService>();

// ───── Swagger & CORS ─────
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(c =>
// {
//     c.SwaggerDoc("v1", new OpenApiInfo { Title = "Portfolio API", Version = "v1" });
// });
// builder.Services.AddCors(opt =>
//     opt.AddPolicy("Frontend", p =>
//         p.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));
// 
// var app = builder.Build();
// app.UseCors("Frontend");
// app.UseSwagger();
// app.UseSwaggerUI();
// 
// // ───── Endpoints ─────
// app.MapPost("/trades/buy", (BuyRequestDto dto, TradeService svc) =>
//     Results.Ok(svc.Buy(dto)));
// 
// app.MapPost("/trades/sell", (SellRequestDto dto, TradeService svc) =>
//     Results.Ok(svc.Sell(dto)));
// 
// app.Run("http://localhost:5255");
