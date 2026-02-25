using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesGen;

namespace Spix.AppInfra.ModelConfig.EntitiesGen;

public class ServiceClientConfig : IEntityTypeConfiguration<ServiceClient>
{
    public void Configure(EntityTypeBuilder<ServiceClient> builder)
    {
        builder.HasKey(e => e.ServiceClientId);
        builder.Property(x => x.ServiceClientId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.ServiceName }).IsUnique();
        builder.Property(e => e.Costo).HasPrecision(18, 2);
        builder.Property(e => e.Price).HasPrecision(18, 2);
        //Borrado En Cascada
        builder.HasOne(e => e.Tax).WithMany(c => c.ServiceClients).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ServiceCategory).WithMany(c => c.ServiceClients).OnDelete(DeleteBehavior.Restrict);
    }
}