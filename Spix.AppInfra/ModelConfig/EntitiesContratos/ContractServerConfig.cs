using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractServerConfig : IEntityTypeConfiguration<ContractServer>
{
    public void Configure(EntityTypeBuilder<ContractServer> builder)
    {
        builder.HasKey(e => e.ContractServerId);
        builder.Property(x => x.ContractServerId).HasDefaultValueSql("NEWSEQUENTIALID()");
        //UN servidor por contrato, garantizado por la base.
        //
        //Antes el indice era sobre el par (ContractClientId, ServerId), que solo impide
        //repetir el mismo par: dos altas simultaneas con servidores distintos pasaban las
        //dos, y el tipo de control que resuelve el sistema quedaba indefinido.
        builder.HasIndex(e => e.ContractClientId).IsUnique();

        //Evitar el borrado en cascada
        builder.HasOne(e => e.ContractClient).WithMany(c => c.ContractServers).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Server).WithMany(c => c.ContractServers).OnDelete(DeleteBehavior.Restrict);
    }
}