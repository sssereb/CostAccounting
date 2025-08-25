// Infrastructure/EfCore/PortfolioDbContext.cs
using Microsoft.EntityFrameworkCore;
using PortfolioApp.Domain; // если доменные классы лежат в этом неймспейсе

namespace PortfolioApp.Infrastructure.EfCore;

public class PortfolioDbContext : DbContext
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : base(options) {}

    // если используете доменные сущности напрямую — DbSet по ним:
    public DbSet<Asset>   Assets  => Set<Asset>();
    public DbSet<Lot>     Lots    => Set<Lot>();
    public DbSet<Trade>   Trades  => Set<Trade>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // выносим детальную конфигурацию в отдельные классы:
        b.ApplyConfiguration(new Configs.AssetConfig());
        b.ApplyConfiguration(new Configs.LotConfig());
        b.ApplyConfiguration(new Configs.TradeConfig());

        base.OnModelCreating(b);
    }
}