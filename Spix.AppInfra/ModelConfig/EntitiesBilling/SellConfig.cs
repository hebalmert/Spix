using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesBilling;

namespace Spix.AppInfra.ModelConfig.EntitiesBilling;

public class SellConfig : IEntityTypeConfiguration<Sell>
{
    public void Configure(EntityTypeBuilder<Sell> builder)
    {
        builder.HasKey(e => e.SellId);
        builder.Property(e => e.SellId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.InvoiceNumber }).IsUnique();
        builder.Property(e => e.DateSell).HasColumnType("date");
        builder.Property(e => e.DateCancelled).HasColumnType("date");
        builder.Property(e => e.DatePaid).HasColumnType("date");

        builder.HasOne(e => e.Corporation).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ContractClient).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Client).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.BillingNote).WithMany(e => e.Sells).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.BillingNoteOne).WithMany(e => e.Sells).OnDelete(DeleteBehavior.Restrict);
    }
}
