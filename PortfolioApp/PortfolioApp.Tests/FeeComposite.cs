using PortfolioApp.Application;        // FeeRegistration
using PortfolioApp.Application.Fees;  // FixedPerTrade, PercentOfValue, FixedPerShare, FeeComposite
using PortfolioApp.Domain;            // FeeDirection
using Xunit;

namespace PortfolioApp.Tests;

public class FeeCompositeTests
{
    private const int Qty   = 100;    // кол-во акций
    private const decimal Px = 50m;   // цена за акцию

    // фиксированные значения комиссий
    private const decimal FixedTrade  = 10m;   // $10 за сделку
    private const decimal PerShareFee = 0.5m;  // $0,50 за акцию
    private const decimal Percent     = 0.02m; // 2 %

    // ----------- 12 сценариев -----------
    public static IEnumerable<object[]> Cases => new[]
{
    // ---------- FixedPerTrade ----------
    new object[] { "FixedPerTrade-Buy",          FeeDirection.Buy,   FeeDirection.Buy,   true,  false, false, 10m  },
    new object[] { "FixedPerTrade-Sell",         FeeDirection.Sell,  FeeDirection.Sell,  true,  false, false, 10m  },
    new object[] { "FixedPerTrade-Both→Buy",     FeeDirection.Both,  FeeDirection.Buy,   true,  false, false, 10m  },
    new object[] { "FixedPerTrade-Both→Sell",    FeeDirection.Both,  FeeDirection.Sell,  true,  false, false, 10m  },
    // ❌ негатив
    new object[] { "FixedPerTrade-Mismatch1",    FeeDirection.Buy,   FeeDirection.Sell,  true,  false, false, 0m   },
    new object[] { "FixedPerTrade-Mismatch2",    FeeDirection.Sell,  FeeDirection.Buy,   true,  false, false, 0m   },

    // ---------- PercentOfValue ----------
    new object[] { "PercentOfValue-Buy",         FeeDirection.Buy,   FeeDirection.Buy,   false, false, true,  100m },
    new object[] { "PercentOfValue-Sell",        FeeDirection.Sell,  FeeDirection.Sell,  false, false, true,  100m },
    new object[] { "PercentOfValue-Both→Buy",    FeeDirection.Both,  FeeDirection.Buy,   false, false, true,  100m },
    new object[] { "PercentOfValue-Both→Sell",   FeeDirection.Both,  FeeDirection.Sell,  false, false, true,  100m },
    // ❌ негатив
    new object[] { "PercentOfValue-Mismatch1",   FeeDirection.Buy,   FeeDirection.Sell,  false, false, true,  0m   },
    new object[] { "PercentOfValue-Mismatch2",   FeeDirection.Sell,  FeeDirection.Buy,   false, false, true,  0m   },

    // ---------- FixedPerShare ----------
    new object[] { "FixedPerShare-Buy",          FeeDirection.Buy,   FeeDirection.Buy,   false, true,  false, 50m  },
    new object[] { "FixedPerShare-Sell",         FeeDirection.Sell,  FeeDirection.Sell,  false, true,  false, 50m  },
    new object[] { "FixedPerShare-Both→Buy",     FeeDirection.Both,  FeeDirection.Buy,   false, true,  false, 50m  },
    new object[] { "FixedPerShare-Both→Sell",    FeeDirection.Both,  FeeDirection.Sell,  false, true,  false, 50m  },
    // ❌ негатив
    new object[] { "FixedPerShare-Mismatch1",    FeeDirection.Buy,   FeeDirection.Sell,  false, true,  false, 0m   },
    new object[] { "FixedPerShare-Mismatch2",    FeeDirection.Sell,  FeeDirection.Buy,   false, true,  false, 0m   },

    // ---------- Все комиссии вместе ----------
    new object[] { "ALL-Buy",                    FeeDirection.Buy,   FeeDirection.Buy,   true,  true,  true,  160m },
    new object[] { "ALL-Sell",                   FeeDirection.Sell,  FeeDirection.Sell,  true,  true,  true,  160m },
    new object[] { "ALL-Both→Buy",               FeeDirection.Both,  FeeDirection.Buy,   true,  true,  true,  160m },
    new object[] { "ALL-Both→Sell",              FeeDirection.Both,  FeeDirection.Sell,  true,  true,  true,  160m },
    // ❌ негатив (полный «облом»)
    new object[] { "ALL-Mismatch1",              FeeDirection.Buy,   FeeDirection.Sell,  true,  true,  true,  0m   },
    new object[] { "ALL-Mismatch2",              FeeDirection.Sell,  FeeDirection.Buy,   true,  true,  true,  0m   },
};

    [Theory(DisplayName = "FeeComposite.CalсAll — 12 вариантов")]
    [MemberData(nameof(Cases))]
    public void CalcAll_ReturnsExpectedTotal(
        string         _caseName,
        FeeDirection   registrationDir,
        FeeDirection   callDir,
        bool           useFixedTrade,
        bool           usePerShare,
        bool           usePercent,
        decimal        expected)
    {
        // ---------- arrange ----------
        var regs = new List<FeeRegistration>();

        if (useFixedTrade)
            regs.Add(new FeeRegistration(new FixedPerTrade(FixedTrade),        registrationDir));
        if (usePerShare)
            regs.Add(new FeeRegistration(new FixedPerShare(PerShareFee),       registrationDir));
        if (usePercent)
            regs.Add(new FeeRegistration(new PercentOfValue(Percent),          registrationDir));

        var composite = new FeeComposite(regs);

        // ---------- act ----------
        var total = composite
                    .CalcAll(Qty, Px, callDir)   // возвращает IReadOnlyList<Fee>
                    .Sum(f => f.Amount);         // суммируем Amount

        // ---------- assert ----------
        Assert.Equal(expected, total);
    }
}
