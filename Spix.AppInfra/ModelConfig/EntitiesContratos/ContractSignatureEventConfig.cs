using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractSignatureEventConfig : IEntityTypeConfiguration<ContractSignatureEvent>
{
    public void Configure(EntityTypeBuilder<ContractSignatureEvent> builder)
    {
        builder.HasKey(e => e.ContractSignatureEventId);
        builder.Property(e => e.ContractSignatureEventId).HasDefaultValueSql("NEWSEQUENTIALID()");

        //La bitacora se lee siempre en orden por contrato y tipo de documento
        builder.HasIndex(e => new { e.ContractClientId, e.DocumentType, e.CreatedAt });

        builder.HasOne(e => e.Corporation)
            .WithMany()
            .HasForeignKey(e => e.CorporationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ContractClient)
            .WithMany()
            .HasForeignKey(e => e.ContractClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
