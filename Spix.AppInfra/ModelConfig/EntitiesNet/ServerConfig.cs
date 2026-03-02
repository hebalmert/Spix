using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesNet;

namespace Spix.AppInfra.ModelConfig.EntitiesNet;

public class ServerConfig : IEntityTypeConfiguration<Server>
{
    public void Configure(EntityTypeBuilder<Server> builder)
    {
        builder.HasKey(e => e.ServerId);
        builder.Property(x => x.ServerId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.ServerName, e.CorporationId }).IsUnique();
        builder.HasIndex(e => new { e.IpNetworkId, e.CorporationId }).IsUnique();
        //Evitar el borrado en cascada
        builder.HasOne(e => e.IpNetwork).WithMany(c => c.Servers).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Mark).WithMany(c => c.Servers).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.MarkModel).WithMany(c => c.Servers).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Zone).WithMany(c => c.Servers).OnDelete(DeleteBehavior.Restrict);
    }
}