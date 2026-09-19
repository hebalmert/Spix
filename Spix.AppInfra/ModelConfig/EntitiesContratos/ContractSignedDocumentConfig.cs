using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractSignedDocumentConfig : IEntityTypeConfiguration<ContractSignedDocument>
{
    public void Configure(EntityTypeBuilder<ContractSignedDocument> builder)
    {
        builder.HasKey(e => e.ContractSignedDocumentId);
        builder.Property(e => e.ContractSignedDocumentId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.ContractClientId, e.ContractDocumentTemplateId }).IsUnique();
        builder.Property(e => e.DateCreated).HasColumnType("date");

        //La hora exacta de la firma es evidencia: DateSigned guarda fecha y hora completas

        //Identificador publico de la firma: unico cuando existe
        builder.HasIndex(e => e.VerificationCode)
            .IsUnique()
            .HasFilter("[VerificationCode] IS NOT NULL");

        builder.HasOne(e => e.Corporation)
            .WithMany()
            .HasForeignKey(e => e.CorporationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ContractClient)
            .WithMany(e => e.ContractSignedDocuments)
            .HasForeignKey(e => e.ContractClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ContractDocumentTemplate)
            .WithMany(e => e.ContractSignedDocuments)
            .HasForeignKey(e => e.ContractDocumentTemplateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
