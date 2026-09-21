using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractExemptConfig : IEntityTypeConfiguration<ContractExempt>
{
    public void Configure(EntityTypeBuilder<ContractExempt> builder)
    {
        builder.HasKey(e => e.ContractExemptId);
        builder.Property(e => e.ContractExemptId).HasDefaultValueSql("NEWSEQUENTIALID()");

        //Se busca por contrato (para cerrar la exoneracion abierta) y por fecha (para los reportes)
        builder.HasIndex(e => new { e.ContractClientId, e.DateEnded });
        builder.HasIndex(e => new { e.CorporationId, e.DateExempt });

        builder.HasOne(e => e.Corporation)
            .WithMany()
            .HasForeignKey(e => e.CorporationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ContractClient)
            .WithMany()
            .HasForeignKey(e => e.ContractClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Client)
            .WithMany()
            .HasForeignKey(e => e.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
