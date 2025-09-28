using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.Entities;

namespace Spix.AppInfra.ModelConfig.Entities;

public class CityConfig : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.HasIndex(e => e.CityId);
        builder.HasIndex(e => new { e.Name, e.StateId }).IsUnique();
        builder.Property(e => e.Name).UseCollation("Latin1_General_CI_AS"); //Para poderlo volver Collation CI
        //Proteccion de Borrado en Cascada
        builder.HasOne(e => e.State).WithMany(e => e.Cities).OnDelete(DeleteBehavior.Restrict);
    }
}