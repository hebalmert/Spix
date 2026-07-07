using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractDocumentTemplateConfig : IEntityTypeConfiguration<ContractDocumentTemplate>
{
    public void Configure(EntityTypeBuilder<ContractDocumentTemplate> builder)
    {
        builder.HasKey(e => e.ContractDocumentTemplateId);
        builder.Property(e => e.ContractDocumentTemplateId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.DocumentType, e.Name }).IsUnique();
        builder.Property(e => e.DateCreated).HasColumnType("date");

        builder.HasOne(e => e.Corporation)
            .WithMany()
            .HasForeignKey(e => e.CorporationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
