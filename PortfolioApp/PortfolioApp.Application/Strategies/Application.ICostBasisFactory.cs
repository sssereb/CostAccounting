
using Microsoft.Extensions.DependencyInjection;

namespace PortfolioApp.Application.Strategies;

public interface ICostBasisFactory
{
    ICostBasisStrategy Get(CostBasisMethod method);
}
public sealed class CostBasisFactory : ICostBasisFactory
{
    private readonly IServiceProvider _sp;
    public CostBasisFactory(IServiceProvider sp) => _sp = sp;
    public ICostBasisStrategy Get(CostBasisMethod m) => m switch
    {
        CostBasisMethod.FIFO => _sp.GetRequiredService<FifoStrategy>(),
        CostBasisMethod.LIFO => _sp.GetRequiredService<LifoStrategy>(),
        _ => _sp.GetRequiredService<AverageCostStrategy>()
    };
}