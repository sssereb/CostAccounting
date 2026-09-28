
namespace PortfolioApp.Application;

/// <param name="SoldCostPerShare">Cost basis of the sold shares, buy fees included.</param>
/// <param name="RemainingCostPerShare">Cost basis of the shares left, buy fees included.</param>
/// <param name="GrossProfit">Proceeds minus purchase price; no fees on either side.</param>
/// <param name="NetProfit">Proceeds minus cost basis (buy fees) minus sell fees.</param>
public record SaleResult
(
    int RemainingShares, 
    decimal SoldCostPerShare, 
    decimal RemainingCostPerShare, 
    decimal GrossProfit,
    decimal NetProfit
);
