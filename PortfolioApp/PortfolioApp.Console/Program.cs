
using Microsoft.Extensions.DependencyInjection;
using PortfolioApp.Domain;
using PortfolioApp.Application;
using PortfolioApp.Application.Repositories.InMemory;
using PortfolioApp.Application.Repositories.Interfaces;
using PortfolioApp.Application.Strategies;

public class Program
{
    public static void Main(string[] args)
    {
        // --- DI bootstrapping ---
        var services = new ServiceCollection().AddPortfolioCore();
        using var provider = services.BuildServiceProvider();
        
        var tradeService = provider.GetRequiredService<TradeService>();
        var assetRepo = provider.GetRequiredService<IAssetRepository>();
        // --- demo data ---
        var asset = assetRepo.Create("MSFT");
         tradeService.Buy(asset.Id, 100, 20m, new DateTime(2025, 1, 1));
         tradeService.Buy(asset.Id, 150, 30m, new DateTime(2025, 2, 1));
         tradeService.Buy(asset.Id, 120, 10m, new DateTime(2025, 3, 1));

        // tradeService.Buy(asset.Id, 100, 20m, new DateTime(2025, 1, 1)); 
        // tradeService.Buy(asset.Id, 200, 30m, new DateTime(2025, 2, 1));


        while (true)
        {
            Console.Write("\nCommand (buy/sell/list/exit): ");
            var cmd = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (cmd is "exit" or "") break;
            try
            {
                switch (cmd)
                {
                    case "buy":
                        Console.Write("Ticker Qty Price DateTime(yyyy/mm/dd): ");
                        var b = Console.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        tradeService.Buy(assetRepo.GetByTicker(b![0])?.Id ?? assetRepo.Create(b[0]).Id,
                                         int.Parse(b[1]), decimal.Parse(b[2]), DateTime.Parse(b[3]));
                        Console.WriteLine("✓ Buy recorded");
                        break;
                    case "sell":
                        Console.Write("Ticker Qty Price Method(FIFO/LIFO/AVG) DateTime(yyyy/mm/dd): ");
                        var s = Console.ReadLine()?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        var method = s![3].ToUpper() switch
                        {
                            "FIFO" => CostBasisMethod.FIFO,
                            "LIFO" => CostBasisMethod.LIFO,
                            _ => CostBasisMethod.Average
                        };
                        var sale = tradeService.Sell(assetRepo.GetByTicker(s[0])!.Id,
                                                     int.Parse(s[1]), decimal.Parse(s[2]), method, DateTime.Parse(s[4]));
                        Console.WriteLine($"SoldCost: {sale.SoldCostPerShare:F2}, Left: {sale.RemainingShares}, GrossProfit: {sale.GrossProfit:F2},  NetProfit: {sale.NetProfit:F2}");
                        break;

                    case "list":
                        var lotRepo = provider.GetRequiredService<ILotRepository>();
                        foreach (var a in assetRepo.GetAll())
                        {
                            Console.WriteLine($"\n{a.Ticker}:");
                            var lots = lotRepo.GetForAsset(a.Id).OrderBy(l => l.PurchaseDate).ToList();
                            if (!lots.Any())
                            {
                                Console.WriteLine("   (no lots)");
                                continue;
                            }
                            foreach (var l in lots)
                            {
                                Console.WriteLine($"   {l.PurchaseDate:yyyy-MM-dd}  QtyRemain={l.QtyRemain}  RawUnitCost={l.RawUnitCost}, UnitCost={l.UnitCost}");
                            }
                            var total = lots.Sum(l => l.QtyRemain);
                            var cps = tradeService.GetRemaininCostPerShare(a.Id);
                            Console.WriteLine($"   -- Total remaining: {total}, Cost Per Share : {cps}");
                        }
                        break;
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
        }
    }
}