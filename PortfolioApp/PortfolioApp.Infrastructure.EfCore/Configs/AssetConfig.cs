using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortfolioApp.Domain;

namespace PortfolioApp.Infrastructure.EfCore.Configs;

public sealed class AssetConfig : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> e)
    {
        e.ToTable("assets");
        e.HasKey(x => x.Id);

        e.Property(x => x.Ticker)
            .IsRequired()
            .HasMaxLength(32);
        e.HasIndex(x => x.Ticker).IsUnique();

    }
}