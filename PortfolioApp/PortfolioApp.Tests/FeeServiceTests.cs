using PortfolioApp.Application;
using PortfolioApp.Application.Fees;
using PortfolioApp.Domain;

namespace PortfolioApp.Tests;

// 100 shares at 100.00: fixed per trade 10, fixed per share 0.5 (= 50), 1% of value (= 100).
public class FeeServiceTests
{
    private const int     Qty   = 100;
    private const decimal Price = 100m;

    public static IEnumerable<object[]> Cases => new[]
    {
        //             rule direction     call direction     trade  share  pct    expected
        new object[] { FeeDirection.Buy,  FeeDirection.Buy,  true,  false, false, 10m },
        new object[] { FeeDirection.Sell, FeeDirection.Sell, true,  false, false, 10m },
        new object[] { FeeDirection.Both, FeeDirection.Buy,  true,  false, false, 10m },
        new object[] { FeeDirection.Both, FeeDirection.Sell, true,  false, false, 10m },
        new object[] { FeeDirection.Buy,  FeeDirection.Sell, true,  false, false, 0m },
        new object[] { FeeDirection.Sell, FeeDirection.Buy,  true,  false, false, 0m },

        new object[] { FeeDirection.Buy,  FeeDirection.Buy,  false, false, true,  100m },
        new object[] { FeeDirection.Sell, FeeDirection.Sell, false, false, true,  100m },
        new object[] { FeeDirection.Both, FeeDirection.Buy,  false, false, true,  100m },
        new object[] { FeeDirection.Both, FeeDirection.Sell, false, false, true,  100m },
        new object[] { FeeDirection.Buy,  FeeDirection.Sell, false, false, true,  0m },
        new object[] { FeeDirection.Sell, FeeDirection.Buy,  false, false, true,  0m },

        new object[] { FeeDirection.Buy,  FeeDirection.Buy,  false, true,  false, 50m },
        new object[] { FeeDirection.Sell, FeeDirection.Sell, false, true,  false, 50m },
        new object[] { FeeDirection.Both, FeeDirection.Buy,  false, true,  false, 50m },
        new object[] { FeeDirection.Both, FeeDirection.Sell, false, true,  false, 50m },
        new object[] { FeeDirection.Buy,  FeeDirection.Sell, false, true,  false, 0m },
        new object[] { FeeDirection.Sell, FeeDirection.Buy,  false, true,  false, 0m },

        new object[] { FeeDirection.Buy,  FeeDirection.Buy,  true,  true,  true,  160m },
        new object[] { FeeDirection.Sell, FeeDirection.Sell, true,  true,  true,  160m },
        new object[] { FeeDirection.Both, FeeDirection.Buy,  true,  true,  true,  160m },
        new object[] { FeeDirection.Both, FeeDirection.Sell, true,  true,  true,  160m },
        new object[] { FeeDirection.Buy,  FeeDirection.Sell, true,  true,  true,  0m },
        new object[] { FeeDirection.Sell, FeeDirection.Buy,  true,  true,  true,  0m }
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void CalcAll_AppliesRulesForTheCallDirection(
        FeeDirection ruleDirection, FeeDirection callDirection,
        bool perTrade, bool perShare, bool percent, decimal expected)
    {
        var rules = new List<FeeRegistration>();
        if (perTrade) rules.Add(new(FeeType.FixedPerTrade, 10m,   ruleDirection));
        if (perShare) rules.Add(new(FeeType.FixedPerShare, 0.5m,  ruleDirection));
        if (percent)  rules.Add(new(FeeType.Percent,       0.01m, ruleDirection));

        var service = new FeeService(new MemoryFeeRuleProvider(rules));

        Assert.Equal(expected, service.CalcAll(Qty, Price, callDirection).Sum(f => f.Amount));
    }
}
