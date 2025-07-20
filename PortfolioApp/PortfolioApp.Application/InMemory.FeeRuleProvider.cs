// PortfolioApp.Infrastructure/Fees/MemoryFeeRuleProvider.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PortfolioApp.Application;
using PortfolioApp.Domain;          // FeeDirection
using PortfolioApp.Application.Fees; // FeeType, FeeRegistration

namespace PortfolioApp.Infrastructure.Fees;

/// <summary>
/// Простейший провайдер правил комиссий, работающий в памяти.
/// Поддерживает параллельное чтение и один поток записи.
/// </summary>
public sealed class MemoryFeeRuleProvider : IFeeRuleProvider
{
    private readonly ReaderWriterLockSlim _rw = new();
    private List<FeeRegistration> _rules;

    /// <param name="initialRules">Стартовый набор правил (может быть пустым).</param>
    public MemoryFeeRuleProvider(IEnumerable<FeeRegistration> initialRules)
        => _rules = initialRules?.ToList() ?? new();

    /// <summary>Вернуть текущий список правил (копия, чтобы внешние модификации не влияли).</summary>
    public IReadOnlyCollection<FeeRegistration> GetRules()
    {
        _rw.EnterReadLock();
        try   { return _rules.ToList(); }          // отдаём копию
        finally { _rw.ExitReadLock(); }
    }

    /// <summary>Полностью заменить список правил.</summary>
    /// <exception cref="InvalidOperationException">Если переданы дубли (Type+Amount+Direction).</exception>
    public void SetRules(IEnumerable<FeeRegistration> rules)
    {
        if (rules is null) throw new ArgumentNullException(nameof(rules));
        var fresh = rules.ToList();

        // --- проверка на дубли -------------------------------------------------
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
