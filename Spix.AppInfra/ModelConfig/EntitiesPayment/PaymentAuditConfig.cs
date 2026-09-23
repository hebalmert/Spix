using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesPayment;

namespace Spix.AppInfra.ModelConfig.EntitiesPayment;

public class PaymentAuditConfig : IEntityTypeConfiguration<PaymentAudit>
{
    public void Configure(EntityTypeBuilder<PaymentAudit> builder)
    {
        builder.HasKey(e => e.PaymentAuditId);
        builder.Property(e => e.PaymentAuditId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(e => e.DateEvent).HasColumnType("datetime2");

        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.TaxAmount).HasPrecision(18, 2);
        builder.Property(e => e.Total).HasPrecision(18, 2);

        //Es la tabla que mas crece: se lee por caja (corporacion y fecha), por contrato
        //y por documento, y siempre paginada.
        builder.HasIndex(e => new { e.CorporationId, e.DateEvent });
        builder.HasIndex(e => new { e.ContractClientId, e.DateEvent });
        builder.HasIndex(e => e.ReferenceId);

        builder.HasOne(e => e.Corporation)
            .WithMany()
            .HasForeignKey(e => e.CorporationId)
            .OnDelete(DeleteBehavior.Restrict);

        //El documento se puede borrar (un adelanto sin facturar); su rastro se queda
        builder.HasOne(e => e.ContractClient)
            .WithMany()
            .HasForeignKey(e => e.ContractClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Client)
            .WithMany()
            .HasForeignKey(e => e.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
