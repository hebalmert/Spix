using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesNet;

namespace Spix.AppInfra.ModelConfig.EntitiesNet;

public class IpNetConfig : IEntityTypeConfiguration<IpNet>
{
    public void Configure(EntityTypeBuilder<IpNet> builder)
    {
        builder.HasKey(e => e.IpNetId);
        builder.Property(x => x.IpNetId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.Ip, e.CorporationId }).IsUnique();
    }
}