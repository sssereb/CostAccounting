// PortfolioApp.Console.Tests/Application/FeeCompositeTests.cs
using PortfolioApp.Application;
using PortfolioApp.Application.Fees;
using PortfolioApp.Domain;

namespace PortfolioApp.Tests;

public class FeeCompositeTests
{
    private const int     Qty   = 100;
    private const decimal Price = 100m;

    private static IFeeCalculator CreateCalc(FeeType type, decimal amt) => type switch
    {
        FeeType.FixedPerTrade  => new FixedPerTrade (amt),
        FeeType.FixedPerShare  => new FixedPerShare (amt),
        FeeType.Percent => new PercentOfValue(amt),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    public static IEnumerable<object[]> Cases => new[]
    {
        // name                       regDir  callDir  FT  FS  PV   expected
        new object[]{ "FixedPerTrade-Buy",       FeeDirection.Buy,  FeeDirection.Buy,   true,  false, false, 10m  },
        new object[]{ "FixedPerTrade-Sell",      FeeDirection.Sell, FeeDirection.Sell,  true,  false, false, 10m  },
        new object[]{ "FixedPerTrade-Both→Buy",  FeeDirection.Both, FeeDirection.Buy,   true,  false, false, 10m  },
        new object[]{ "FixedPerTrade-Both→Sell", FeeDirection.Both, FeeDirection.Sell,  true,  false, false, 10m  },
        new object[]{ "FixedPerTrade-Mismatch1", FeeDirection.Buy,  FeeDirection.Sell,  true,  false, false, 0m   },
        new object[]{ "FixedPerTrade-Mismatch2", FeeDirection.Sell, FeeDirection.Buy,   true,  false, false, 0m   },

        new object[]{ "PercentOfValue-Buy",      FeeDirection.Buy,  FeeDirection.Buy,   false, false, true,  100m },
        new object[]{ "PercentOfValue-Sell",     FeeDirection.Sell, FeeDirection.Sell,  false, false, true,  100m },
        new object[]{ "PercentOfValue-Both→Buy", FeeDirection.Both, FeeDirection.Buy,   false, false, true,  100m },
        new object[]{ "PercentOfValue-Both→Sell",FeeDirection.Both, FeeDirection.Sell,  false, false, true,  100m },
        new object[]{ "PercentOfValue-Mismatch1",FeeDirection.Buy,  FeeDirection.Sell,  false, false, true,  0m   },
        new object[]{ "PercentOfValue-Mismatch2",FeeDirection.Sell, FeeDirection.Buy,   false, false, true,  0m   },

        new object[]{ "FixedPerShare-Buy",       FeeDirection.Buy,  FeeDirection.Buy,   false, true,  false, 50m  },
        new object[]{ "FixedPerShare-Sell",      FeeDirection.Sell, FeeDirection.Sell,  false, true,  false, 50m  },
        new object[]{ "FixedPerShare-Both→Buy",  FeeDirection.Both, FeeDirection.Buy,   false, true,  false, 50m  },
        new object[]{ "FixedPerShare-Both→Sell", FeeDirection.Both, FeeDirection.Sell,  false, true,  false, 50m  },
        new object[]{ "FixedPerShare-Mismatch1", FeeDirection.Buy,  FeeDirection.Sell,  false, true,  false, 0m   },
        new object[]{ "FixedPerShare-Mismatch2", FeeDirection.Sell, FeeDirection.Buy,   false, true,  false, 0m   },

        new object[]{ "ALL-Buy",                 FeeDirection.Buy,  FeeDirection.Buy,   true,  true,  true,  160m },
        new object[]{ "ALL-Sell",                FeeDirection.Sell, FeeDirection.Sell,  true,  true,  true,  160m },
        new object[]{ "ALL-Both→Buy",            FeeDirection.Both, FeeDirection.Buy,   true,  true,  true,  160m },
        new object[]{ "ALL-Both→Sell",           FeeDirection.Both, FeeDirection.Sell,  true,  true,  true,  160m },
        new object[]{ "ALL-Mismatch1",           FeeDirection.Buy,  FeeDirection.Sell,  true,  true,  true,  0m   },
        new object[]{ "ALL-Mismatch2",           FeeDirection.Sell, FeeDirection.Buy,   true,  true,  true,  0m   }
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void CalcAll_ReturnsExpected(
        string           _caseName,      
        FeeDirection     regDir,
        FeeDirection     callDir,
        bool             addFT,
        bool             addFS,
        bool             addPV,
        decimal          expected)
    {
        var rules = new List<FeeRegistration>();
        if (addFT) rules.Add(new(FeeType.FixedPerTrade,  10m,   regDir));
        if (addFS) rules.Add(new(FeeType.FixedPerShare,  0.5m,  regDir)); // 0.5 × 100 = 50
        if (addPV) rules.Add(new(FeeType.Percent, 0.01m, regDir)); // 1 %

        var calcs = rules
            .Where(r => r.Direction == callDir || r.Direction == FeeDirection.Both)
            .Select(r => (IFeeCalculator)CreateCalc(r.Type, r.Amount));

        var composite = new FeeComposite(calcs);

        var total = composite.CalcAll(Qty, Price, callDir)
                             .Sum(f => f.Amount);

        Assert.Equal(expected, total);
    }
}
