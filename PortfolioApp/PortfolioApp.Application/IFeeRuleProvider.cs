using PortfolioApp.Application;

public interface IFeeRuleProvider
{
    IReadOnlyCollection<FeeRegistration> GetRules();
    void SetRules(IEnumerable<FeeRegistration> rules);
}