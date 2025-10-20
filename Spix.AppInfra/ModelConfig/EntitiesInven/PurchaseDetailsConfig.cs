using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesInven;

namespace Spix.AppInfra.ModelConfig.EntitiesInven;

public class PurchaseDetailsConfig : IEntityTypeConfiguration<PurchaseDetail>
{
    public void Configure(EntityTypeBuilder<PurchaseDetail> builder)
    {
        builder.HasKey(e => e.PurchaseDetailId);
        builder.Property(x => x.PurchaseDetailId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.ProductId, e.PurchaseId }).IsUnique();
        //Evitar el borrado en cascada
        builder.HasOne(e => e.Product).WithMany(c => c.PurchaseDetails).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Purchase).WithMany(c => c.PurchaseDetails).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ProductCategory).WithMany(c => c.PurchaseDetails).OnDelete(DeleteBehavior.Restrict);
    }
}