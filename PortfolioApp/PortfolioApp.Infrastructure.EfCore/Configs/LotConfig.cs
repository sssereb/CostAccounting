// Infrastructure/EfCore/Configs/LotConfig.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortfolioApp.Domain;

namespace PortfolioApp.Infrastructure.EfCore.Configs;

public sealed class LotConfig : IEntityTypeConfiguration<Lot>
{
    public void Configure(EntityTypeBuilder<Lot> e)
    {
        e.ToTable("lots");
        e.HasKey(x => x.Id);
        e.HasIndex(x => new { x.AssetId});
        e.Property(x => x.PurchaseDate).IsRequired();
        e.Property(x => x.AssetId).IsRequired();
        e.Property(x => x.QtyInitial).IsRequired();
        e.Property(x => x.QtyRemain).IsRequired();
        e.Property(x => x.RawUnitCost).IsRequired(); 
        e.Property(x => x.UnitCost).IsRequired(); 
    }
}