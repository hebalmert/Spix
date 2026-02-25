using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesGen;

namespace Spix.AppInfra.ModelConfig.EntitiesGen;

public class ProductConfig : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(e => e.ProductId);
        builder.Property(x => x.ProductId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.ProductName }).IsUnique();
        builder.Property(e => e.Costo).HasPrecision(18, 2);
        builder.Property(e => e.Price).HasPrecision(18, 2);
        //Borrado En Cascada
        builder.HasOne(e => e.Tax).WithMany(c => c.Products).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ProductCategory).WithMany(c => c.Products).OnDelete(DeleteBehavior.Restrict);
    }
}