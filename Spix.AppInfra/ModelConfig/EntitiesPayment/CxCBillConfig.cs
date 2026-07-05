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
        builder.Property(e => e.DateNote).HasColumnType("date");
        builder.Property(e => e.DatePaid).HasColumnType("date");
        builder.Property(e => e.DateCancelled).HasColumnType("date");
        builder.Property(e => e.Total).HasPrecision(18, 2);
        builder.Property(e => e.Balance).HasPrecision(18, 2);

        builder.HasOne(e => e.Corporation).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Client).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ContractClient).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Sell).WithMany(e => e.CxCBills).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.BillingNoteOne).WithMany(e => e.CxCBills).OnDelete(DeleteBehavior.Restrict);
    }
}
