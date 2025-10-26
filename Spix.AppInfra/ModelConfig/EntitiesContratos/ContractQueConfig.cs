using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Core.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractQueConfig : IEntityTypeConfiguration<ContractQue>
{
    public void Configure(EntityTypeBuilder<ContractQue> builder)
    {
        builder.HasKey(e => e.ContractQueId);
        builder.HasIndex(e => new { e.ContractClientId, e.IpNetId }).IsUnique();

        //Evitar el borrado en cascada
        builder.HasOne(e => e.ContractClient).WithMany(c => c.ContractQues).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Server).WithMany(c => c.ContractQues).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Plan).WithMany(c => c.ContractQues).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.IpNet).WithMany(c => c.ContractQues).OnDelete(DeleteBehavior.Restrict);
    }
}