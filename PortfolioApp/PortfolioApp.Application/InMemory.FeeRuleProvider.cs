// PortfolioApp.Infrastructure/Fees/MemoryFeeRuleProvider.cs
using PortfolioApp.Application;

namespace PortfolioApp.Infrastructure.Fees;

/// <summary>
/// Simple in-memory fee rule provider.
/// Supports concurrent readers and a single writer.
/// </summary>
public sealed class MemoryFeeRuleProvider : IFeeRuleProvider
{
    private readonly ReaderWriterLockSlim _rw = new();
    private List<FeeRegistration> _rules;

    /// <param name="initialRules">Initial rules (may be empty).</param>
    public MemoryFeeRuleProvider(IEnumerable<FeeRegistration> initialRules)
        => _rules = initialRules?.ToList() ?? new();

    /// <summary>Returns a copy of the current rules so callers cannot modify them.</summary>
    public IReadOnlyCollection<FeeRegistration> GetRules()
    {
        _rw.EnterReadLock();
        try   { return _rules.ToList(); }
        finally { _rw.ExitReadLock(); }
    }

    /// <summary>Replaces the whole rule list.</summary>
    /// <exception cref="InvalidOperationException">When duplicate rules (Type+Amount+Direction) are passed.</exception>
    public void SetRules(IEnumerable<FeeRegistration> rules)
    {
        if (rules is null) throw new ArgumentNullException(nameof(rules));
        var fresh = rules.ToList();

        // --- duplicate check --------------------------------------------------
        var dup = fresh.GroupBy(r => (r.Type, r.Amount, r.Direction))
                       .FirstOrDefault(g => g.Count() > 1);
        if (dup != null)
        {
            var (type, amt, dir) = dup.Key;
            throw new InvalidOperationException(
                $"Duplicate fee rule detected: {type} {amt} {dir}");
        }
        // ----------------------------------------------------------------------

        _rw.EnterWriteLock();
        try   { _rules = fresh; }
        finally { _rw.ExitWriteLock(); }
    }
}
