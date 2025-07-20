// PortfolioApp.Application/Fees/FeeService.cs
// -------------------------------------------
// Сервис-фасад: считает все комиссии для сделки, опираясь на
// актуальный набор правил (FeeRegistration) из IFeeRuleProvider.
// -------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using PortfolioApp.Domain;          // Fee, FeeDirection

namespace PortfolioApp.Application.Fees;

/// <summary>
/// Высоко-уровневый сервис расчёта комиссий.
/// На каждый вызов формирует набор конкретных калькуляторов
/// на основании правил (Type + Amount + Direction), хранящихся
/// в <see cref="IFeeRuleProvider"/>.
/// </summary>
///
///
public interface IFeeService
{
    /// <param name="qty">Количество акций / лотов.</param>
    /// <param name="price">Цена одной акции (или контракта).</param>
    /// <param name="direction">Buy / Sell (или Both, если нужно).</param>
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

        // 1) берём правила под нужное направление
        var activeRules = _rules.GetRules()
            .Where(r => r.Direction == direction || r.Direction == FeeDirection.Both)
            .ToList();

        if (activeRules.Count == 0)
            return Array.Empty<Fee>();

        // 2) создаём калькуляторы «на лету»
        var calculators = activeRules
            .Select(FeeCalculatorFactory.Create)  // FeeRegistration → IFeeCalculator
            .ToArray();

        // 3) складываем комиссии
        var composite = new FeeComposite(calculators);
        return composite.CalcAll(qty, price, direction);
    }
}