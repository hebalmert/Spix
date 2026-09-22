using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesPayment;

namespace Spix.AppInfra.ModelConfig.EntitiesPayment;

public class PrePaymentDetailConfig : IEntityTypeConfiguration<PrePaymentDetail>
{
    public void Configure(EntityTypeBuilder<PrePaymentDetail> builder)
    {
        builder.HasKey(e => e.PrePaymentDetailId);
        builder.Property(e => e.PrePaymentDetailId).HasDefaultValueSql("NEWSEQUENTIALID()");

        //Un servicio no se puede adelantar dos veces
        builder.HasIndex(e => new { e.CorporationId, e.ServiceRequestDetailId }).IsUnique()
            .HasFilter("[ServiceRequestDetailId] IS NOT NULL");

        builder.Property(e => e.TaxRate).HasPrecision(5, 2);
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.TaxAmount).HasPrecision(18, 2);
        builder.Property(e => e.PriceWithTax).HasPrecision(18, 2);

        //Las lineas se borran con su pago adelantado mientras no este facturado
        builder.HasOne(e => e.PrePayment).WithMany(e => e.PrePaymentDetails).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Corporation).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ServiceRequest).WithMany().OnDelete(DeleteBehavior.Restrict);
    }
}
