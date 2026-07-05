using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesBilling;

namespace Spix.AppInfra.ModelConfig.EntitiesBilling;

public class BillingNoteConfig : IEntityTypeConfiguration<BillingNote>
{
    public void Configure(EntityTypeBuilder<BillingNote> builder)
    {
        builder.HasKey(e => e.BillingNoteId);
        builder.Property(e => e.BillingNoteId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.YearNumber, e.MonthType }).IsUnique();
        builder.Property(e => e.DateBill).HasColumnType("date");
        builder.Property(e => e.DateCreated).HasColumnType("date");

        builder.HasOne(e => e.Corporation).WithMany().OnDelete(DeleteBehavior.Restrict);
    }
}
