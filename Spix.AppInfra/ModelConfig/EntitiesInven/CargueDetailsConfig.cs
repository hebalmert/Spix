using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesInven;

namespace Spix.AppInfra.ModelConfig.EntitiesInven;

public class CargueDetailsConfig : IEntityTypeConfiguration<CargueDetail>
{
    public void Configure(EntityTypeBuilder<CargueDetail> builder)
    {
        builder.HasKey(e => e.CargueDetailId);
        builder.Property(x => x.CargueDetailId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.MacWlan, e.CorporationId }).IsUnique();
        //Evitar el borrado en cascada
        builder.HasOne(e => e.Cargue).WithMany(c => c.CargueDetails).OnDelete(DeleteBehavior.Restrict);
    }
}