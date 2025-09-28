using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.Entities;

namespace Spix.AppInfra.ModelConfig.Entities;

public class StateConfig : IEntityTypeConfiguration<State>
{
    public void Configure(EntityTypeBuilder<State> builder)
    {
        builder.HasIndex(e => e.StateId);
        builder.HasIndex(e => new { e.Name, e.CountryId }).IsUnique();
        builder.Property(e => e.Name).UseCollation("Latin1_General_CI_AS");
        //Proteccion de Borrado en Cascada
        builder.HasOne(e => e.Country).WithMany(e => e.States).OnDelete(DeleteBehavior.Restrict);
    }
}