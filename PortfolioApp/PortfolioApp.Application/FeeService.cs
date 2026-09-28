using PortfolioApp.Domain;          // Fee, FeeDirection

namespace PortfolioApp.Application.Fees;

/// <summary>
/// High-level fee calculation service.
/// On every call it builds concrete calculators from the rules
/// (Type + Amount + Direction) stored in <see cref="IFeeRuleProvider"/>.
/// </summary>
///
///
public interface IFeeService
{
    /// <param name="qty">Number of shares.</param>
    /// <param name="price">Price per share (or contract).</param>
    /// <param name="direction">Buy / Sell (or Both if needed).</param>
    IReadOnlyList<Fee> CalcAll(int qty, decimal price, FeeDirection direction);
}

public sealed class FeeService : IFeeService
{
    private readonly IFeeRuleProvider _rules;

    public FeeService(IFeeRuleProvider rules) =>
        _rules = rules ?? throw new ArgumentNullException(nameof(rules));

    /// <inheritdoc />
    public IReadOnlyList<Fee> CalcAll(int qty, decimal price, FeeDirection direction)
    {
        if (qty   <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        if (price <  0) throw new ArgumentOutOfRangeException(nameof(price));

        // 1) rules for the requested direction
        var activeRules = _rules.GetRules()
            .Where(r => r.Direction == direction || r.Direction == FeeDirection.Both)
            .ToList();

        if (activeRules.Count == 0)
            return Array.Empty<Fee>();

        // 2) build calculators on the fly
        var calculators = activeRules
            .Select(FeeCalculatorFactory.Create)  // FeeRegistration → IFeeCalculator
            .ToArray();

        // 3) one fee per calculator
        var composite = new FeeComposite(calculators);
        return composite.CalcAll(qty, price);
    }
}