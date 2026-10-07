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

        //La bodega donde esta hoy el equipo, y la linea de traslado que lo tiene reservado.
        //Las dos opcionales: un serial recien cargado todavia no viajo a ninguna parte.
        builder.HasOne(e => e.ProductStorage)
            .WithMany()
            .HasForeignKey(e => e.ProductStorageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TransferDetails)
            .WithMany()
            .HasForeignKey(e => e.TransferDetailsId)
            .OnDelete(DeleteBehavior.Restrict);

        //Se busca por bodega y estado en cada traslado y en el reporte
        builder.HasIndex(e => new { e.CorporationId, e.ProductStorageId, e.Status });
    }
}