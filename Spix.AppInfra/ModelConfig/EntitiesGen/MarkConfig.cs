using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesGen;

namespace Spix.AppInfra.ModelConfig.EntitiesGen;

public class MarkConfig : IEntityTypeConfiguration<Mark>
{
    public void Configure(EntityTypeBuilder<Mark> builder)
    {
        builder.HasKey(e => e.MarkId);
        builder.Property(x => x.MarkId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(x => new { x.MarkName, x.CorporationId }).IsUnique();
    }
}