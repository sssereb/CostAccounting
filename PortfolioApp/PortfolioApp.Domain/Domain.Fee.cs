namespace PortfolioApp.Domain;


public enum FeeDirection { None = 0, Buy = 1, Sell = 2, Both = Buy | Sell }

public enum FeeType
{
    FixedPerTrade,
    FixedPerShare,
    Percent
}

public record Fee (FeeType FeeType, decimal Amount);

