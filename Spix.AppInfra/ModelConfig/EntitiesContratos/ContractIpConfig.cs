using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractIpConfig : IEntityTypeConfiguration<ContractIp>
{
    public void Configure(EntityTypeBuilder<ContractIp> builder)
    {
        builder.HasKey(e => e.ContractIpId);
        builder.HasIndex(e => new { e.ContractClientId, e.IpNetId }).IsUnique();

        //Evitar el borrado en cascada
        builder.HasOne(e => e.ContractClient).WithMany(c => c.ContractIps).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.IpNet).WithMany(c => c.ContractIps).OnDelete(DeleteBehavior.Restrict);
    }
}