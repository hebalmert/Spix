using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesPayment;

namespace Spix.AppInfra.ModelConfig.EntitiesPayment;

public class ContractorAccountPayableConfig : IEntityTypeConfiguration<ContractorAccountPayable>
{
    public void Configure(EntityTypeBuilder<ContractorAccountPayable> builder)
    {
        builder.HasKey(e => e.ContractorAccountPayableId);
        builder.Property(e => e.ContractorAccountPayableId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.CxCBillDetailId }).IsUnique();
        builder.Property(e => e.DateCreated).HasColumnType("date");
        builder.Property(e => e.DatePaid).HasColumnType("date");
        builder.Property(e => e.Rate).HasPrecision(5, 2);
        builder.Property(e => e.BaseAmount).HasPrecision(18, 2);
        builder.Property(e => e.Total).HasPrecision(18, 2);
        builder.Property(e => e.Balance).HasPrecision(18, 2);

        builder.HasOne(e => e.Contractor)
            .WithMany(e => e.ContractorAccountPayables)
            .HasForeignKey(e => e.ContractorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ContractClient)
            .WithMany()
            .HasForeignKey(e => e.ContractClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CxCBill)
            .WithMany(e => e.ContractorAccountPayables)
            .HasForeignKey(e => e.CxCBillId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CxCBillDetail)
            .WithMany(e => e.ContractorAccountPayables)
            .HasForeignKey(e => e.CxCBillDetailId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Corporation)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
