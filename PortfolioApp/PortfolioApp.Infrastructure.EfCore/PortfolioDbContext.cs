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

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        BumpLotVersions();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken ct = default)
    {
        BumpLotVersions();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, ct);
    }

    private void BumpLotVersions()
    {
        foreach (var entry in ChangeTracker.Entries<Lot>().Where(e => e.State == EntityState.Modified))
            entry.Property(l => l.Version).CurrentValue += 1;
    }
}