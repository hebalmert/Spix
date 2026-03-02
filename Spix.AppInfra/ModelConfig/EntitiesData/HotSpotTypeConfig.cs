using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesData;

namespace Spix.AppInfra.ModelConfig.EntitiesData;

public class HotSpotTypeConfig : IEntityTypeConfiguration<HotSpotType>
{
    public void Configure(EntityTypeBuilder<HotSpotType> builder)
    {
        builder.HasKey(e => e.HotSpotTypeId);
        builder.HasIndex(e => e.TypeName).IsUnique();
    }
}