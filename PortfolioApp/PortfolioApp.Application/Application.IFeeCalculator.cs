using PortfolioApp.Domain;

namespace PortfolioApp.Application.Fees;

public interface IFeeCalculator
{
    Fee Calc(int qty, decimal pricePerShare);
}

/* --- конкретные правила --- */
public sealed class FixedPerTrade : IFeeCalculator
{
    private readonly decimal _fee;          // например 1.00$
    public FixedPerTrade(decimal fee) => _fee = fee;
    public Fee Calc(int _, decimal __) => new(FeeType.FixedPerTrade, _fee);
}

public sealed class FixedPerShare : IFeeCalculator
{
    private readonly decimal _perShare;     // напр. 0.005$ за акцию
    public FixedPerShare(decimal cents) => _perShare = cents;
    public Fee Calc(int qty, decimal __) => new(FeeType.FixedPerShare, qty * _perShare);
}

public sealed class PercentOfValue : IFeeCalculator
{
    private readonly decimal _pct;          // 0.003m = 0.3 %
    public PercentOfValue(decimal pct) => _pct = pct;
    public Fee Calc(int qty, decimal px)=> new(FeeType.Percent, qty*px*_pct);
}

public sealed class FeeComposite
{
    private readonly IEnumerable<IFeeCalculator> _calcs;

    public FeeComposite(IEnumerable<IFeeCalculator> calculators) =>
        _calcs = calculators;

    public IReadOnlyList<Fee> CalcAll(int qty, decimal price, FeeDirection dir) =>
        _calcs.Select(c => c.Calc(qty, price)).ToList();
}