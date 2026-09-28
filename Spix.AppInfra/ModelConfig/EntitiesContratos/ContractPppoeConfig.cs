using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractPppoeConfig : IEntityTypeConfiguration<ContractPppoe>
{
    public void Configure(EntityTypeBuilder<ContractPppoe> builder)
    {
        builder.HasKey(e => e.ContractPppoeId);
        builder.Property(x => x.ContractPppoeId).HasDefaultValueSql("NEWSEQUENTIALID()");

        //Una sola credencial por contrato: en PPPoE el cliente entra con un usuario y uno solo
        builder.HasIndex(e => e.ContractClientId).IsUnique();

        //El usuario tiene que ser unico en el equipo: es la llave con la que autentica.
        //Y el servidor ya es de una corporacion, asi que el aislamiento queda implicito.
        builder.HasIndex(e => new { e.ServerId, e.Usuario }).IsUnique();

        //Evitar el borrado en cascada
        builder.HasOne(e => e.ContractClient).WithMany(c => c.ContractPppoes).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Server).WithMany(c => c.ContractPppoes).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.IpNet).WithMany(c => c.ContractPppoes).OnDelete(DeleteBehavior.Restrict);
    }
}
