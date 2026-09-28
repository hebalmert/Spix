using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractOltConfig : IEntityTypeConfiguration<ContractOlt>
{
    public void Configure(EntityTypeBuilder<ContractOlt> builder)
    {
        builder.HasKey(e => e.ContractOltId);
        builder.Property(x => x.ContractOltId).HasDefaultValueSql("NEWSEQUENTIALID()");

        //UNA OLT por contrato: el cliente entra por una sola. El indice va sobre el
        //contrato y no sobre el par, que solo impediria repetir la misma.
        builder.HasIndex(e => e.ContractClientId).IsUnique();

        //Evitar el borrado en cascada
        builder.HasOne(e => e.ContractClient).WithMany(c => c.ContractOlts).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Olt).WithMany(c => c.ContractOlts).OnDelete(DeleteBehavior.Restrict);
    }
}
