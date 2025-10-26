using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Core.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractServerConfig : IEntityTypeConfiguration<ContractServer>
{
    public void Configure(EntityTypeBuilder<ContractServer> builder)
    {
        builder.HasKey(e => e.ContractServerId);
        builder.HasIndex(e => new { e.ContractClientId, e.ServerId }).IsUnique();

        //Evitar el borrado en cascada
        builder.HasOne(e => e.ContractClient).WithMany(c => c.ContractServers).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Server).WithMany(c => c.ContractServers).OnDelete(DeleteBehavior.Restrict);
    }
}