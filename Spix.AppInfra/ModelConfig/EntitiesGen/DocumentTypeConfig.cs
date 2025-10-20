using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesGen;

namespace Spix.AppInfra.ModelConfig.EntitiesGen;

public class DocumentTypeConfig : IEntityTypeConfiguration<DocumentType>
{
    public void Configure(EntityTypeBuilder<DocumentType> builder)
    {
        builder.HasKey(e => e.DocumentTypeId);
        builder.Property(x => x.DocumentTypeId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.DocumentName, e.CorporationId }).IsUnique();
    }
}