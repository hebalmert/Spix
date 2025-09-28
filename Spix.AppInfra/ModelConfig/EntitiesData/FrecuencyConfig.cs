using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesData;

namespace Spix.AppInfra.ModelConfig.EntitiesData;

public class FrecuencyConfig : IEntityTypeConfiguration<Frecuency>
{
    public void Configure(EntityTypeBuilder<Frecuency> builder)
    {
        builder.HasKey(e => e.FrecuencyId);
        builder.HasIndex(e => new { e.FrecuencyTypeId, e.FrecuencyName }).IsUnique();
        //Evitar el borrado en cascada
        builder.HasOne(e => e.FrecuencyType).WithMany(c => c.Frecuencies).OnDelete(DeleteBehavior.Restrict);
    }
}