using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesInven;

namespace Spix.AppInfra.ModelConfig.EntitiesInven;

public class TransferDetailSerialConfig : IEntityTypeConfiguration<TransferDetailSerial>
{
    public void Configure(EntityTypeBuilder<TransferDetailSerial> builder)
    {
        builder.HasKey(e => e.TransferDetailSerialId);
        builder.Property(x => x.TransferDetailSerialId).HasDefaultValueSql("NEWSEQUENTIALID()");

        //Por aqui se consulta siempre: los equipos de UNA linea del traslado
        builder.HasIndex(e => new { e.CorporationId, e.TransferDetailsId });

        //El mismo equipo NO se repite dentro de la misma linea
        builder.HasIndex(e => new { e.TransferDetailsId, e.CargueDetailId }).IsUnique();

        //Evitar el borrado en cascada
        builder.HasOne(e => e.TransferDetails).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.CargueDetail).WithMany().OnDelete(DeleteBehavior.Restrict);
    }
}
