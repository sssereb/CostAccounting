namespace PortfolioApp.Application.Fees;

public interface IFeeRuleProvider
{
    IReadOnlyCollection<FeeRegistration> GetRules();
    void SetRules(IEnumerable<FeeRegistration> rules);
}