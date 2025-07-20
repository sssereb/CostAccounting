// Application/Fees/FeeCalculatorFactory.cs

using PortfolioApp.Application;
using PortfolioApp.Application.Fees;
using PortfolioApp.Domain;

public static class FeeCalculatorFactory
{
    public static IFeeCalculator Create(FeeRegistration r) => r.Type switch
    {
        FeeType.FixedPerTrade   => new FixedPerTrade (r.Amount),
        FeeType.FixedPerShare   => new FixedPerShare (r.Amount),
        FeeType.Percent  => new PercentOfValue(r.Amount),
        _ => throw new ArgumentOutOfRangeException(nameof(r.Type))
    };
}