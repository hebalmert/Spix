using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractBindConfig : IEntityTypeConfiguration<ContractBind>
{
    public void Configure(EntityTypeBuilder<ContractBind> builder)
    {
        builder.HasKey(e => e.ContractBindId);
        builder.Property(x => x.ContractBindId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.ContractClientId, e.IpNetId }).IsUnique();

        //Evitar el borrado en cascada
        builder.HasOne(e => e.ContractClient).WithMany(c => c.ContractBinds).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Server).WithMany(c => c.ContractBinds).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.IpNet).WithMany(c => c.ContractBinds).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.CargueDetail).WithMany(c => c.ContractBinds).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.HotSpotType).WithMany(c => c.ContractBinds).OnDelete(DeleteBehavior.Restrict);
    }
}
