using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractNodoConfig : IEntityTypeConfiguration<ContractNode>
{
    public void Configure(EntityTypeBuilder<ContractNode> builder)
    {
        builder.HasKey(e => e.ContractNodeId);
        builder.HasIndex(e => new { e.ContractClientId, e.NodeId }).IsUnique();

        //Evitar el borrado en cascada
        builder.HasOne(e => e.ContractClient).WithMany(c => c.ContractNodes).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Node).WithMany(c => c.ContractNodes).OnDelete(DeleteBehavior.Restrict);
    }
}