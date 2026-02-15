using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.Entities;

namespace Spix.AppInfra.ModelConfig.Entities;

public class ManagerConfig : IEntityTypeConfiguration<Manager>
{
    public void Configure(EntityTypeBuilder<Manager> builder)
    {
        builder.HasKey(e => e.ManagerId);
        builder.HasIndex(e => e.UserName).IsUnique();
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(x => new { x.FirstName, x.LastName, x.NroDocument }).IsUnique();
        builder.Property(e => e.FirstName).UseCollation("Latin1_General_CI_AS");
        builder.Property(e => e.LastName).UseCollation("Latin1_General_CI_AS");
        //Evitar el borrado en cascada
        builder.HasOne(e => e.Corporation).WithMany(c => c.Managers).OnDelete(DeleteBehavior.Restrict);
    }
}