using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesOper;

namespace Spix.AppInfra.ModelConfig.EntitiesOper;

public class ContractorConfig : IEntityTypeConfiguration<Contractor>
{
    public void Configure(EntityTypeBuilder<Contractor> builder)
    {
        builder.HasKey(e => e.ContractorId);
        builder.HasIndex(e => e.UserName).IsUnique();
        builder.HasIndex(e => new { e.CorporationId, e.DocumentTypeId, e.Document }).IsUnique();
        builder.HasIndex(e => new { e.CorporationId, e.FirstName, e.LastName }).IsUnique();
        builder.Property(e => e.Rate).HasPrecision(15, 2);
        builder.Property(e => e.DateCreated).HasColumnType("date");
        //Evitar el borrado en cascada
        builder.HasOne(e => e.DocumentType).WithMany(c => c.Contractors).OnDelete(DeleteBehavior.Restrict);
    }
}