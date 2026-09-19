using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractSignatureCodeConfig : IEntityTypeConfiguration<ContractSignatureCode>
{
    public void Configure(EntityTypeBuilder<ContractSignatureCode> builder)
    {
        builder.HasKey(e => e.ContractSignatureCodeId);
        builder.Property(e => e.ContractSignatureCodeId).HasDefaultValueSql("NEWSEQUENTIALID()");

        //Se busca siempre el ultimo codigo vigente de un contrato y tipo de documento
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
