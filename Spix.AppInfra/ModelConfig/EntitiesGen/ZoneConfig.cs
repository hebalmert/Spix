using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesGen;

namespace Spix.AppInfra.ModelConfig.EntitiesGen;

public class ZoneConfig : IEntityTypeConfiguration<Zone>
{
    public void Configure(EntityTypeBuilder<Zone> builder)
    {
        builder.HasKey(e => e.ZoneId);
        builder.Property(x => x.ZoneId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.StateId, e.CityId, e.ZoneName }).IsUnique();
        //Evitar el Borrado en Cascada
        builder.HasOne(e => e.state).WithMany(e => e.Zones).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.city).WithMany(e => e.Zones).OnDelete(DeleteBehavior.Restrict);
    }
}