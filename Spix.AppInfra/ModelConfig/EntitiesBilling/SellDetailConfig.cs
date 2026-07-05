using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesBilling;

namespace Spix.AppInfra.ModelConfig.EntitiesBilling;

public class SellDetailConfig : IEntityTypeConfiguration<SellDetail>
{
    public void Configure(EntityTypeBuilder<SellDetail> builder)
    {
        builder.HasKey(e => e.SellDetailId);
        builder.Property(e => e.SellDetailId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(e => e.Quantity).HasPrecision(18, 2);
        builder.Property(e => e.TaxRate).HasPrecision(5, 2);
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.TaxAmount).HasPrecision(18, 2);
        builder.Property(e => e.Price).HasPrecision(18, 2);

        builder.HasOne(e => e.Corporation).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Sell).WithMany(e => e.SellDetails).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Tax).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ServiceRequest).WithMany().OnDelete(DeleteBehavior.Restrict);
    }
}
