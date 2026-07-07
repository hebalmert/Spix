using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractDocumentTemplateFieldConfig : IEntityTypeConfiguration<ContractDocumentTemplateField>
{
    public void Configure(EntityTypeBuilder<ContractDocumentTemplateField> builder)
    {
        builder.HasKey(e => e.ContractDocumentTemplateFieldId);
        builder.Property(e => e.ContractDocumentTemplateFieldId).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.HasOne(e => e.ContractDocumentTemplate)
            .WithMany(e => e.ContractDocumentTemplateFields)
            .HasForeignKey(e => e.ContractDocumentTemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
