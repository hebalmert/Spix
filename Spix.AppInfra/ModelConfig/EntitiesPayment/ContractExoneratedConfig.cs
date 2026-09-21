using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesPayment;

namespace Spix.AppInfra.ModelConfig.EntitiesPayment;

public class ContractExoneratedConfig : IEntityTypeConfiguration<ContractExonerated>
{
    public void Configure(EntityTypeBuilder<ContractExonerated> builder)
    {
        builder.HasKey(e => e.ContractExoneratedId);
        builder.Property(e => e.ContractExoneratedId).HasDefaultValueSql("NEWSEQUENTIALID()");
        //Un contrato no puede tener dos exoneraciones VIGENTES del mismo mes; las cerradas
        //quedan como historia y por eso no entran en el indice.
        builder.HasIndex(e => new { e.CorporationId, e.ContractClientId, e.YearNumber, e.MonthType })
            .IsUnique()
            .HasFilter("[DateEnded] IS NULL");

        builder.Property(e => e.DateExonerated).HasColumnType("date");
        builder.Property(e => e.DateEnded).HasColumnType("datetime2");
        builder.Property(e => e.DateBilled).HasColumnType("date");
        builder.Property(e => e.TaxRate).HasPrecision(5, 2);
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.PriceWithTax).HasPrecision(18, 2);

        builder.HasOne(e => e.Corporation).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Client).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ContractClient).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Plan).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.CxCBill).WithMany(e => e.ContractExonerateds).OnDelete(DeleteBehavior.Restrict);
    }
}
