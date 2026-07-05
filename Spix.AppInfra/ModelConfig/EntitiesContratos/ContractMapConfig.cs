using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractMapConfig : IEntityTypeConfiguration<ContractMap>
{
    public void Configure(EntityTypeBuilder<ContractMap> builder)
    {
        builder.HasKey(e => e.ContractMapId);
        builder.Property(e => e.ContractMapId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => e.ContractClientId).IsUnique();
        builder.Property(e => e.Latitude).HasPrecision(12, 7);
        builder.Property(e => e.Longitude).HasPrecision(12, 7);

        builder.HasOne(e => e.ContractClient).WithMany(e => e.ContractMaps).OnDelete(DeleteBehavior.Restrict);
    }
}
