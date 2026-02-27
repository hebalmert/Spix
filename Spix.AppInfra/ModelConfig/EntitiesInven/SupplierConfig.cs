using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesInven;

namespace Spix.AppInfra.ModelConfig.EntitiesInven;

public class SupplierConfig : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.HasKey(e => e.SupplierId);
        builder.Property(x => x.SupplierId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.Name }).IsUnique();
        builder.HasIndex(e => new { e.CorporationId, e.Document, e.DocumentTypeId }).IsUnique();
        //Evitar el borrado en cascada
        builder.HasOne(e => e.State).WithMany(c => c.Suppliers).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.City).WithMany(c => c.Suppliers).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DocumentType).WithMany(c => c.Suppliers).OnDelete(DeleteBehavior.Restrict);
    }
}