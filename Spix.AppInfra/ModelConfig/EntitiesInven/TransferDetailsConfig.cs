using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesInven;

namespace Spix.AppInfra.ModelConfig.EntitiesInven;

public class TransferDetailsConfig : IEntityTypeConfiguration<TransferDetails>
{
    public void Configure(EntityTypeBuilder<TransferDetails> builder)
    {
        builder.HasKey(e => e.TransferDetailsId);
        builder.Property(x => x.TransferDetailsId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.ProductId, e.TransferId }).IsUnique();
        //Evitar el borrado en cascada
        builder.HasOne(e => e.Product).WithMany(c => c.TransferDetails).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Transfer).WithMany(c => c.TransferDetails).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ProductCategory).WithMany(c => c.TransferDetails).OnDelete(DeleteBehavior.Restrict);
    }
}