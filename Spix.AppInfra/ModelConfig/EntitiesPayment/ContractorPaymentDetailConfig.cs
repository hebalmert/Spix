using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesPayment;

namespace Spix.AppInfra.ModelConfig.EntitiesPayment;

public class ContractorPaymentDetailConfig : IEntityTypeConfiguration<ContractorPaymentDetail>
{
    public void Configure(EntityTypeBuilder<ContractorPaymentDetail> builder)
    {
        builder.HasKey(e => e.ContractorPaymentDetailId);
        builder.Property(e => e.ContractorPaymentDetailId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.ContractorPaymentId, e.ContractorAccountPayableId }).IsUnique();
        builder.Property(e => e.Payment).HasPrecision(18, 2);

        builder.HasOne(e => e.ContractorPayment)
            .WithMany(e => e.ContractorPaymentDetails)
            .HasForeignKey(e => e.ContractorPaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ContractorAccountPayable)
            .WithMany(e => e.ContractorPaymentDetails)
            .HasForeignKey(e => e.ContractorAccountPayableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
