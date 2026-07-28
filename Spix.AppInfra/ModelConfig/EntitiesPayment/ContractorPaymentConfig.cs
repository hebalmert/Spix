using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesPayment;

namespace Spix.AppInfra.ModelConfig.EntitiesPayment;

public class ContractorPaymentConfig : IEntityTypeConfiguration<ContractorPayment>
{
    public void Configure(EntityTypeBuilder<ContractorPayment> builder)
    {
        builder.HasKey(e => e.ContractorPaymentId);
        builder.Property(e => e.ContractorPaymentId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(e => e.DatePayment).HasColumnType("date");
        builder.Property(e => e.PaymentNumber).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Total).HasPrecision(18, 2);

        builder.HasIndex(e => new { e.CorporationId, e.PaymentNumber })
            .IsUnique()
            .HasFilter("[PaymentNumber] <> ''");

        builder.HasOne(e => e.Contractor)
            .WithMany(e => e.ContractorPayments)
            .HasForeignKey(e => e.ContractorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Corporation)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
