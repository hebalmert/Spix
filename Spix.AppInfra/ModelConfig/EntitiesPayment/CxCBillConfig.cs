using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesPayment;

namespace Spix.AppInfra.ModelConfig.EntitiesPayment;

public class CxCBillConfig : IEntityTypeConfiguration<CxCBill>
{
    public void Configure(EntityTypeBuilder<CxCBill> builder)
    {
        builder.HasKey(e => e.CxCBillId);
        builder.Property(e => e.CxCBillId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.CollectionNote }).IsUnique();

        //Garantia del lanzamiento por lotes: un contrato no puede tener DOS notas vivas del
        //mismo periodo. Si un reintento trata de repetirlo, la base lo rechaza.
        //Las anuladas quedan fuera del indice: por eso el filtro.
        builder.HasIndex(e => new { e.CorporationId, e.ContractClientId, e.YearNumber, e.MonthType })
            .IsUnique()
            .HasFilter("[Cancelled] = 0 AND [YearNumber] > 0");

        //Se consulta mucho "que contratos ya tienen nota de este mes"
        builder.HasIndex(e => new { e.CorporationId, e.YearNumber, e.MonthType });
        //El listado va por corporacion y fecha de nota
        builder.HasIndex(e => new { e.CorporationId, e.DateNote });
        builder.Property(e => e.DateNote).HasColumnType("date");
        builder.Property(e => e.DatePaid).HasColumnType("date");
        builder.Property(e => e.DateCancelled).HasColumnType("date");
        builder.Property(e => e.UsuarioOwnerCancelled).HasMaxLength(150);
        builder.Property(e => e.Total).HasPrecision(18, 2);
        builder.Property(e => e.Balance).HasPrecision(18, 2);

        builder.HasOne(e => e.Corporation).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Client).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ContractClient).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Sell).WithMany(e => e.CxCBills).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.BillingNoteOne).WithMany(e => e.CxCBills).OnDelete(DeleteBehavior.Restrict);
    }
}
