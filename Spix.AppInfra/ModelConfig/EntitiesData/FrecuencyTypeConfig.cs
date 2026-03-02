using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesData;

namespace Spix.AppInfra.ModelConfig.EntitiesData;

public class FrecuencyTypeConfig : IEntityTypeConfiguration<FrecuencyType>
{
    public void Configure(EntityTypeBuilder<FrecuencyType> builder)
    {
        builder.HasKey(e => e.FrecuencyTypeId);
        builder.HasIndex(e => e.TypeName).IsUnique();
    }
}