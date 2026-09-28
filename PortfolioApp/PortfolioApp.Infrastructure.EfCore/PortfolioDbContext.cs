// Infrastructure/EfCore/PortfolioDbContext.cs
using Microsoft.EntityFrameworkCore;
using PortfolioApp.Domain;

namespace PortfolioApp.Infrastructure.EfCore;

public class PortfolioDbContext : DbContext
{
    public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : base(options) {}

    public DbSet<Asset>   Assets  => Set<Asset>();
    public DbSet<Lot>     Lots    => Set<Lot>();
    public DbSet<Trade>   Trades  => Set<Trade>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // detailed mapping lives in separate configuration classes
        b.ApplyConfiguration(new Configs.AssetConfig());
        b.ApplyConfiguration(new Configs.LotConfig());
        b.ApplyConfiguration(new Configs.TradeConfig());

        base.OnModelCreating(b);
    }
}