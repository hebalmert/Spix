using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesInven;

namespace Spix.AppInfra.ModelConfig.EntitiesInven;

public class ProductStorageConfig : IEntityTypeConfiguration<ProductStorage>
{
    public void Configure(EntityTypeBuilder<ProductStorage> builder)
    {
        builder.HasKey(e => e.ProductStorageId);
        builder.Property(x => x.ProductStorageId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.StorageName }).IsUnique();
        //Borrado En Cascada
        //Evitar el borrado en cascada
        builder.HasOne(x => x.State).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.City) .WithMany(x => x.ProductStorages).OnDelete(DeleteBehavior.Restrict);
    }
}