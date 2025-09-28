using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitesSoftSec;

namespace Spix.AppInfra.ModelConfig.EntitesSoftSec;

public class UsuarioConfig : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.HasKey(e => e.UsuarioId);
        builder.HasIndex(e => e.UserName).IsUnique();
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(x => new { x.FullName, x.Nro_Document, x.CorporationId }).IsUnique();
        builder.Property(e => e.FullName).UseCollation("Latin1_General_CI_AS"); //Para poderlo volver Collation CI
        //Evitar el borrado en cascada
        builder.HasOne(e => e.Corporation).WithMany(c => c.Usuarios).OnDelete(DeleteBehavior.Restrict);
    }
}