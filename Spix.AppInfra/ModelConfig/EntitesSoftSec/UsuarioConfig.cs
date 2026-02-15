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
        builder.HasIndex(x => new { x.FirstName, x.LastName, x.Nro_Document, x.CorporationId }).IsUnique();
        builder.Property(e => e.FirstName).UseCollation("Latin1_General_CI_AS");
        builder.Property(e => e.LastName).UseCollation("Latin1_General_CI_AS");
        //Evitar el borrado en cascada
        builder.HasOne(e => e.Corporation).WithMany(c => c.Usuarios).OnDelete(DeleteBehavior.Restrict);
    }
}