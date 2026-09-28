namespace PortfolioApp.Application.Strategies;

public interface ICostBasisFactory
{
    ICostBasisStrategy Get(CostBasisMethod method);
}
public sealed class CostBasisFactory : ICostBasisFactory
{
    private readonly IReadOnlyDictionary<CostBasisMethod, ICostBasisStrategy> _map;

    public CostBasisFactory(IEnumerable<ICostBasisStrategy> strategies)
        => _map = strategies.ToDictionary(s => s.Method);

    public ICostBasisStrategy Get(CostBasisMethod method) =>
        _map.TryGetValue(method, out var strat)
            ? strat
            : throw new NotSupportedException($"No strategy for {method}");
}
