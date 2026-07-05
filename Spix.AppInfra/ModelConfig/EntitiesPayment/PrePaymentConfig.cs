using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesPayment;

namespace Spix.AppInfra.ModelConfig.EntitiesPayment;

public class PrePaymentConfig : IEntityTypeConfiguration<PrePayment>
{
    public void Configure(EntityTypeBuilder<PrePayment> builder)
    {
        builder.HasKey(e => e.PrePaymentId);
        builder.Property(e => e.PrePaymentId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.ContractClientId, e.YearNumber, e.MonthType }).IsUnique();
        builder.Property(e => e.DatePayment).HasColumnType("date");
        builder.Property(e => e.DateBilled).HasColumnType("date");
        builder.Property(e => e.TaxRate).HasPrecision(5, 2);
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.PriceWithTax).HasPrecision(18, 2);

        builder.HasOne(e => e.Corporation).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Client).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ContractClient).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Plan).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.CxCBill).WithMany(e => e.PrePayments).OnDelete(DeleteBehavior.Restrict);
    }
}
