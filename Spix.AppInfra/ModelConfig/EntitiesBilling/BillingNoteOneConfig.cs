using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesBilling;

namespace Spix.AppInfra.ModelConfig.EntitiesBilling;

public class BillingNoteOneConfig : IEntityTypeConfiguration<BillingNoteOne>
{
    public void Configure(EntityTypeBuilder<BillingNoteOne> builder)
    {
        builder.HasKey(e => e.BillingNoteOneId);
        builder.Property(e => e.BillingNoteOneId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.ContractClientId, e.YearNumber, e.MonthType }).IsUnique();
        builder.Property(e => e.DateBill).HasColumnType("date");
        builder.Property(e => e.DateCreated).HasColumnType("date");

        builder.HasOne(e => e.Corporation).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Client).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ContractClient).WithMany().OnDelete(DeleteBehavior.Restrict);
    }
}
