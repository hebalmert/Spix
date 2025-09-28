using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.Entities;

namespace Spix.AppInfra.ModelConfig.Entities;

public class CorporationConfig : IEntityTypeConfiguration<Corporation>
{
    public void Configure(EntityTypeBuilder<Corporation> builder)
    {
        builder.HasKey(e => e.CorporationId);
        builder.HasIndex(x => new { x.Name, x.NroDocument }).IsUnique();
        builder.Property(e => e.Name).UseCollation("Latin1_General_CI_AS"); //Para poderlo volver Collation CI
        builder.Property(e => e.DateStart).HasColumnType("date"); //Instalar Microsoft.EntityFrameworkCore.Relational
        builder.Property(e => e.DateEnd).HasColumnType("date");   //Instalar Microsoft.EntityFrameworkCore.Relational
        //Evitar el borrado en cascada
        builder.HasOne(e => e.SoftPlan).WithMany(c => c.Corporations).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Country).WithMany(c => c.Corporations).OnDelete(DeleteBehavior.Restrict);
    }
}