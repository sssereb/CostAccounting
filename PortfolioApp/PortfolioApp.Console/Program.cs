using Microsoft.Extensions.DependencyInjection;
using PortfolioApp.Application;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Application.Strategies;
using PortfolioApp.Domain;
using PortfolioApp.Application.Fees;
using PortfolioApp.Infrastructure.EfCore;

public class Program
{
    public static async Task Main(string[] args)
    {
        // --- DI bootstrapping ---
        var services = new ServiceCollection();
        
        services
            .AddPortfolioCore();                 // core services (FeeService, TradeService, etc.)
        
        var storageEnv = Environment.GetEnvironmentVariable("PORTFOLIO_STORAGE") ?? "Sqlite"; // "Sqlite" | "InMemory"
        var useSqlite  = string.Equals(storageEnv, "Sqlite", StringComparison.OrdinalIgnoreCase);

        Console.WriteLine(useSqlite);
        
        if (useSqlite)
        {
            services.AddPortfolioEfCoreSqlite(DbPathResolver.Resolve(Environment.GetEnvironmentVariable("DB_PATH")));
            services.AddPortfolioRepositoriesEfCore();
        }
        else
        {
            services.AddPortfolioRepositoriesInMemory();
        }
        
        using var provider = services.BuildServiceProvider();

        var tradeService = provider.GetRequiredService<TradeService>();
        var assetRepo    = provider.GetRequiredService<IAssetRepository>();
        var lotRepo      = provider.GetRequiredService<ILotRepository>();
        var feeProv      = provider.GetRequiredService<IFeeRuleProvider>();

        // --- demo fee rules ---
        feeProv.SetRules(new[]
        {
            new FeeRegistration(FeeType.FixedPerTrade, 7m,    FeeDirection.Sell),
            new FeeRegistration(FeeType.Percent,       0.01m, FeeDirection.Sell)
        });

        // --- optional demo trades (uncomment if needed) ---
        // var asset = await IAssetRepository.GetOrCreateAsync(assetRepo, "MSFT");
        // await tradeService.BuyAsync(asset.Id, 120, 10m, new DateTime(2025, 3, 1));
        // await tradeService.BuyAsync(asset.Id, 100, 20m, new DateTime(2025, 1, 1));
        // await tradeService.BuyAsync(asset.Id, 150, 30m, new DateTime(2025, 2, 1));

        foreach (var f in feeProv.GetRules())
            Console.WriteLine($"Fee Rules: {f.Direction}, Amount: {f.Amount}, Type: {f.Type}");

        // --- interactive loop ---
        while (true)
        {
            Console.Write("\nCommand (buy/sell/list/exit): ");
            var cmd = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(cmd) || cmd == "exit") break;

            try
            {
                switch (cmd)
                {
                    case "buy":
                    {
                        Console.Write("Ticker Qty Price Date(yyyy/mm/dd): ");
                        var b = Console.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries)!;

                        int qty        = int.Parse(b[1]);
                        decimal price  = decimal.Parse(b[2]);
                        var date       = DateTime.Parse(b[3]);

                        await tradeService.BuyByTickerAsync(b[0], qty, price, date);
                        Console.WriteLine("✓ Buy recorded");
                        break;
                    }
                    case "sell":
                    {
                        Console.Write("Ticker Qty Price Method(FIFO/LIFO/AVG) Date(yyyy/mm/dd): ");
                        var s = Console.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries)!;

                        var selectedAsset = await assetRepo.GetByTickerAsync(s[0])
                                            ?? throw new KeyNotFoundException($"Unknown ticker {s[0]}.");
                        int qty        = int.Parse(s[1]);
                        decimal price  = decimal.Parse(s[2]);
                        var method     = s[3].ToUpper() switch
                        {
                            "FIFO" => CostBasisMethod.FIFO,
                            "LIFO" => CostBasisMethod.LIFO,
                            _      => CostBasisMethod.Average
                        };
                        var date       = DateTime.Parse(s[4]);

                        var sale = await tradeService.SellAsync(selectedAsset.Id, qty, price, method, date);
                        Console.WriteLine($"SoldCost: {sale.SoldCostPerShare:F2}, Left: {sale.RemainingShares}, GrossProfit: {sale.GrossProfit:F2},  NetProfit: {sale.NetProfit:F2}");
                        break;
                    }

                    case "list":
                    {
                        var assets = await assetRepo.GetAllAsync();
                        foreach (var a in assets)
                        {
                            Console.WriteLine($"\n{a.Ticker}:");
                            var lots = (await lotRepo.GetForAssetAsync(a.Id))
                                .OrderBy(l => l.PurchaseDate)
                                .ToList();

                            if (!lots.Any())
                            {
                                Console.WriteLine("   (no lots)");
                                continue;
                            }

                            foreach (var l in lots)
                                Console.WriteLine($"   {l.PurchaseDate:yyyy-MM-dd}  QtyRemain={l.QtyRemain}  RawUnitCost={l.RawUnitCost}, UnitCost={l.UnitCost}");

                            var total = lots.Sum(l => l.QtyRemain);
                            var cps = await tradeService.GetRemainingCostPerShareAsync(a.Id);
                            Console.WriteLine($"   -- Total remaining: {total}, Cost Per Share : {cps}");
                        }
                        break;
                    }

                    default:
                        Console.WriteLine("Unknown command.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
