using Microsoft.EntityFrameworkCore;
using Spix.Domain.EntitiesMK;

namespace Spix.AppInfra.ModelConfig.EntitiesMK;

public class QueueTypeConfig : IEntityTypeConfiguration<QueueType>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<QueueType> builder)
    {
        builder.HasKey(e => e.QueueTypeId);
        builder.Property(e => e.QueueTypeId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.TypeName }).IsUnique();
    }
}
