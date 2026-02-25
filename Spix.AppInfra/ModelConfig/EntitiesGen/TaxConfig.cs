using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesGen;

namespace Spix.AppInfra.ModelConfig.EntitiesGen;

public class TaxConfig : IEntityTypeConfiguration<Tax>
{
    public void Configure(EntityTypeBuilder<Tax> builder)
    {
        builder.HasKey(e => e.TaxId);
        builder.Property(x => x.TaxId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(e => e.Rate).HasPrecision(5, 2);
        builder.HasIndex(e => new { e.CorporationId, e.TaxName }).IsUnique();
        builder.HasIndex(e => new { e.CorporationId, e.Rate }).IsUnique();
    }
}