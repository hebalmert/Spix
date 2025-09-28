using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesData;

namespace Spix.AppInfra.ModelConfig.EntitiesData;

public class ChainTypeConfig : IEntityTypeConfiguration<ChainType>
{
    public void Configure(EntityTypeBuilder<ChainType> builder)
    {
        builder.HasKey(e => e.ChainTypeId);
        builder.HasIndex(e => e.ChainName).IsUnique();
    }
}