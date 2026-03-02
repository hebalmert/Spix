using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesData;

namespace Spix.AppInfra.ModelConfig.EntitiesData;

public class SecurityConfig : IEntityTypeConfiguration<Security>
{
    public void Configure(EntityTypeBuilder<Security> builder)
    {
        builder.HasKey(e => e.SecurityId);
        builder.HasIndex(e => e.SecurityName).IsUnique();
    }
}