
namespace PortfolioApp.Application;

public record SaleResult
(
    int RemainingShares, 
    decimal SoldCostPerShare, 
    decimal RemainingCostPerShare, 
    decimal GrossProfit,
    decimal NetProfit
);
