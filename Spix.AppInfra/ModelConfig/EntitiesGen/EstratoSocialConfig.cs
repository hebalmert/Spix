using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesGen;

namespace Spix.AppInfra.ModelConfig.EntitiesGen;

public class EstratoSocialConfig : IEntityTypeConfiguration<EstratoSocial>
{
    public void Configure(EntityTypeBuilder<EstratoSocial> builder)
    {
        builder.HasKey(e => e.EstratoSocialId);
        builder.Property(x => x.EstratoSocialId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.EstratoSocialName }).IsUnique();
    }
}
