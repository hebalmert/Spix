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
        //Ahora hay DOS caminos de Server a IpNetwork (la del equipo y la local del
        //PPPoE), asi que la llave se dice explicita para que EF no adivine.
        builder.HasOne(e => e.PppLocalIpNet)
               .WithMany()
               .HasForeignKey(e => e.PppLocalIpNetId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Mark).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.MarkModel).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Zone).WithMany(c => c.Servers).OnDelete(DeleteBehavior.Restrict);
    }
}