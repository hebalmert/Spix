using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractSuspendedAuditConfig : IEntityTypeConfiguration<ContractSuspendedAudit>
{
    public void Configure(EntityTypeBuilder<ContractSuspendedAudit> builder)
    {
        builder.HasKey(e => e.ContractSuspendedAuditId);
        builder.Property(x => x.ContractSuspendedAuditId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(e => e.DateModified).HasColumnType("datetime2");
        builder.Property(e => e.UserByName).HasMaxLength(150).IsRequired();
        builder.HasIndex(e => new { e.CorporationId, e.DateModified });

        //Evitar el borrado en cascada
        builder.HasOne(e => e.ContractClient)
            .WithMany(c => c.ContractSuspendedAudits)
            .HasForeignKey(e => e.ContractId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Client)
            .WithMany(c => c.ContractSuspendedAudits)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Corporation)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
