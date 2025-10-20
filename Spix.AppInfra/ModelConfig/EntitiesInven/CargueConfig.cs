using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesInven;

namespace Spix.AppInfra.ModelConfig.EntitiesInven;

public class CargueConfig : IEntityTypeConfiguration<Cargue>
{
    public void Configure(EntityTypeBuilder<Cargue> builder)
    {
        builder.HasKey(e => e.CargueId);
        builder.Property(x => x.CargueId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(e => e.DateCargue).HasColumnType("date");
        //Evitar el borrado en cascada
        builder.HasOne(e => e.Purchase).WithMany(c => c.Cargue).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.PurchaseDetail).WithMany(c => c.Cargue).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Product).WithMany(c => c.Cargue).OnDelete(DeleteBehavior.Restrict);
    }
}