// Infrastructure/EfCore/Configs/TradeConfig.cs

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortfolioApp.Domain;

namespace PortfolioApp.Infrastructure.EfCore.Configs;

public sealed class TradeConfig : IEntityTypeConfiguration<Trade>
{
    public void Configure(EntityTypeBuilder<Trade> e)
    {
        // JSON-настройки
        var json = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        json.Converters.Add(new JsonStringEnumConverter());

        // конвертер для IReadOnlyList<Fee> (nullable, чтобы совпадало с типом PropertyBuilder)
        var converter = new ValueConverter<IReadOnlyList<Fee>?, string>(
            v => JsonSerializer.Serialize(v ?? Array.Empty<Fee>(), json),
            v => (IReadOnlyList<Fee>)(JsonSerializer.Deserialize<List<Fee>>(v, json) ?? new List<Fee>())
        );

        // компаратор без null-propagation (expression tree этого требует)
        var comparer = new ValueComparer<IReadOnlyList<Fee>?>(
            (l, r) => JsonSerializer.Serialize(l ?? Array.Empty<Fee>(), json)
                      == JsonSerializer.Serialize(r ?? Array.Empty<Fee>(), json),
            v => JsonSerializer.Serialize(v ?? Array.Empty<Fee>(), json).GetHashCode(),
            v => (IReadOnlyList<Fee>)(v == null ? new List<Fee>() : v.ToList())
        );

       
        e.ToTable("trades");
        e.HasKey(x => x.Id);

        e.HasIndex(x => new { x.AssetId, x.Date });

        e.Property(x => x.AssetId).IsRequired();
        e.Property(x => x.Date).IsRequired();
        e.Property(x => x.Quantity).IsRequired();
        e.Property(x => x.Price).IsRequired();
        e.Property(x => x.ProfitGross).IsRequired();
        e.Property(x => x.ProfitNet).IsRequired();

        // само свойство
        e.Property(x => x.Fees)
            .HasColumnName("fees_json")
            .HasConversion(converter)
            .HasColumnType("TEXT")
            .HasDefaultValueSql("'[]'")     // ← вместо HasDefaultValue("[]")
            .Metadata.SetValueComparer(comparer);


    }
}