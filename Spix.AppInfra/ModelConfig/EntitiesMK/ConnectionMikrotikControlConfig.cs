using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesMK;

namespace Spix.AppInfra.ModelConfig.EntitiesMK;

public class ConnectionMikrotikControlConfig : IEntityTypeConfiguration<ConnectionMikrotikControl>
{
    public void Configure(EntityTypeBuilder<ConnectionMikrotikControl> builder)
    {
        builder.HasKey(e => e.ConnectionMikrotikControlId);
        builder.Property(e => e.ConnectionMikrotikControlId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => e.CorporationId).IsUnique();
    }
}
