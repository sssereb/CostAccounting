using PortfolioApp.Application.Strategies;

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
}
