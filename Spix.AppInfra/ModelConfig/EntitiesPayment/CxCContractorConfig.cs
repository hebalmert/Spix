using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesPayment;

namespace Spix.AppInfra.ModelConfig.EntitiesPayment;

public class CxCContractorConfig : IEntityTypeConfiguration<CxCContractor>
{
    public void Configure(EntityTypeBuilder<CxCContractor> builder)
    {
        builder.HasKey(e => e.CxCContractorId);
        builder.Property(e => e.CxCContractorId).HasDefaultValueSql("NEWSEQUENTIALID()");

        //El numero de la cuenta no se repite dentro de la corporacion
        builder.HasIndex(e => new { e.CorporationId, e.NoteNumber })
            .IsUnique()
            .HasFilter("[NoteNumber] IS NOT NULL");

        //El listado va por corporacion y fecha; el tablero, por lo que sigue abierto
        builder.HasIndex(e => new { e.CorporationId, e.DateNote });
        builder.HasIndex(e => new { e.CorporationId, e.Paid, e.Cancelled });

        builder.Property(e => e.DateNote).HasColumnType("date");
        builder.Property(e => e.DatePaid).HasColumnType("date");
        builder.Property(e => e.DateCancelled).HasColumnType("date");
        builder.Property(e => e.Total).HasPrecision(18, 2);
        builder.Property(e => e.Balance).HasPrecision(18, 2);

        builder.HasOne(e => e.Corporation).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Contractor).WithMany().OnDelete(DeleteBehavior.Restrict);
    }
}
