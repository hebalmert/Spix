using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractMacConfig : IEntityTypeConfiguration<ContractMac>
{
    public void Configure(EntityTypeBuilder<ContractMac> builder)
    {
        builder.HasKey(e => e.ContractMacId);
        builder.Property(x => x.ContractMacId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.ContractClientId, e.CargueDetailId }).IsUnique();

        //Evitar el borrado en cascada
        builder.HasOne(e => e.ContractClient).WithMany(c => c.ContractMacs).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.CargueDetail).WithMany(c => c.ContractMacs).OnDelete(DeleteBehavior.Restrict);
    }
}
