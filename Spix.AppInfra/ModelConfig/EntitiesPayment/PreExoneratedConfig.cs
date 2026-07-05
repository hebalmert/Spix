using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesPayment;

namespace Spix.AppInfra.ModelConfig.EntitiesPayment;

public class PreExoneratedConfig : IEntityTypeConfiguration<PreExonerated>
{
    public void Configure(EntityTypeBuilder<PreExonerated> builder)
    {
        builder.HasKey(e => e.PreExoneratedId);
        builder.Property(e => e.PreExoneratedId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.ContractClientId, e.YearNumber, e.MonthType }).IsUnique();
        builder.Property(e => e.DateExonerated).HasColumnType("date");
        builder.Property(e => e.DateBilled).HasColumnType("date");
        builder.Property(e => e.TaxRate).HasPrecision(5, 2);
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.PriceWithTax).HasPrecision(18, 2);

        builder.HasOne(e => e.Corporation).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Client).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ContractClient).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Plan).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.CxCBill).WithMany(e => e.PreExonerateds).OnDelete(DeleteBehavior.Restrict);
    }
}
