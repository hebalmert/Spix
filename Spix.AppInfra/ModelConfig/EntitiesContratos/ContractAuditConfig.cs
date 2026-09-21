using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractAuditConfig : IEntityTypeConfiguration<ContractAudit>
{
    public void Configure(EntityTypeBuilder<ContractAudit> builder)
    {
        builder.HasKey(e => e.ContractAuditId);
        builder.Property(e => e.ContractAuditId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(e => e.DateEvent).HasColumnType("datetime2");

        //Se lee la linea de tiempo de UN contrato, y los reportes van por fecha
        builder.HasIndex(e => new { e.ContractClientId, e.DateEvent });
        builder.HasIndex(e => new { e.CorporationId, e.DateEvent });

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
