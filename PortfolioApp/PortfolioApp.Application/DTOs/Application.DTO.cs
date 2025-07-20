using PortfolioApp.Application.Strategies;
using PortfolioApp.Domain;

namespace PortfolioApp.Application.DTOs
{
    public record BuyRequestDto(
        Guid?   AssetId,            // может быть null
        string? Ticker,
        int     Qty,
        decimal Price,
        DateTime Date);

    public record SellRequestDto(
        Guid?   AssetId,
        string? Ticker,
        int     Qty,
        decimal Price,
        CostBasisMethod Method,
        DateTime Date);

    public record LotDto( string Ticker, DateTime PurchaseDate, int QtyRemain, decimal RawUnitCost, decimal UnitCost );
    
    public record FeeRuleDto(
        FeeType      Type,
        decimal      Amount,
        FeeDirection Direction);
    
    public static class FeeRuleMapping
    {
        public static FeeRuleDto ToDto(this FeeRegistration r) =>
            new(r.Type, r.Amount, r.Direction);

        public static FeeRegistration ToDomain(this FeeRuleDto d) =>
            new(d.Type, d.Amount, d.Direction);
    }

}
