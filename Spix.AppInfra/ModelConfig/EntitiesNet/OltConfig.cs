using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesNet;

namespace Spix.AppInfra.ModelConfig.EntitiesNet;

public class OltConfig : IEntityTypeConfiguration<Olt>
{
    public void Configure(EntityTypeBuilder<Olt> builder)
    {
        builder.HasKey(e => e.OltId);
        builder.Property(x => x.OltId).HasDefaultValueSql("NEWSEQUENTIALID()");

        //El nombre no se repite dentro de la misma corporacion
        builder.HasIndex(e => new { e.OltName, e.CorporationId }).IsUnique();

        //Evitar el borrado en cascada
        builder.HasOne(e => e.IpNetwork).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Mark).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.MarkModel).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Zone).WithMany().OnDelete(DeleteBehavior.Restrict);
    }
}
