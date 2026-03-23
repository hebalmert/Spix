using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesOper;

namespace Spix.AppInfra.ModelConfig.EntitiesOper;

public class TechnitianConfig : IEntityTypeConfiguration<Technician>
{
    public void Configure(EntityTypeBuilder<Technician> builder)
    {
        builder.HasKey(e => e.TechnicianId);
        builder.Property(x => x.TechnicianId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => e.UserName).IsUnique();
        builder.HasIndex(e => new { e.CorporationId, e.DocumentTypeId, e.Document }).IsUnique();
        builder.HasIndex(e => new { e.CorporationId, e.FirstName, e.LastName }).IsUnique();
        builder.Property(e => e.DateCreated).HasColumnType("date");
        //Evitar el borrado en cascada
        builder.HasOne(e => e.DocumentType).WithMany(c => c.Technicians).OnDelete(DeleteBehavior.Restrict);
    }
}