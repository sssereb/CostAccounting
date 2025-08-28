using PortfolioApp.Application.Strategies;
using PortfolioApp.Domain;

namespace PortfolioApp.Application.DTOs
{
    public record BuyRequestDto(Guid? AssetId, string? Ticker, int Qty, decimal Price, DateTime Date);

    public record SellRequestDto(
        Guid? AssetId,
        string? Ticker,
        int Qty,
        decimal Price,
        CostBasisMethod Method,
        DateTime Date);

    public record AssetDto(string Ticker, int QtyRemaining, decimal LastPrice);
    public record LotDto(
        string Ticker,
        DateTime PurchaseDate,
        int QtyInitial,
        int QtyRemain,
        decimal RawUnitCost,
        decimal UnitCost);

    public record TradeDto(
        string Ticker,
        DateTime Date,
        int Quantity,
        decimal Price,
        decimal ProfitGross,
        decimal ProfitNet);

    public record FeeRuleDto(FeeType Type, decimal Amount, FeeDirection Direction);

    public static class FeeRuleMapping
    {
        public static FeeRuleDto ToDto(this FeeRegistration r) => new(r.Type, r.Amount, r.Direction);

        public static FeeRegistration ToDomain(this FeeRuleDto d) => new(d.Type, d.Amount, d.Direction);
    }
}