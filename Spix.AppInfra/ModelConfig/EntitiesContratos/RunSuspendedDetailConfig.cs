using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class RunSuspendedDetailConfig : IEntityTypeConfiguration<RunSuspendedDetail>
{
    public void Configure(EntityTypeBuilder<RunSuspendedDetail> builder)
    {
        builder.HasKey(e => e.RunSuspendedDetailId);
        builder.Property(e => e.RunSuspendedDetailId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(e => e.DateUtc).HasColumnType("datetime2");
        builder.Property(e => e.PlanAmount).HasPrecision(18, 2);
        builder.HasIndex(e => new { e.RunSuspendedId, e.ContractClientId }).IsUnique();

        builder.HasOne(e => e.RunSuspended)
            .WithMany(e => e.RunSuspendedDetails)
            .HasForeignKey(e => e.RunSuspendedId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.ContractClient)
            .WithMany(e => e.RunSuspendedDetails)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Client)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.CxCBill)
            .WithMany(e => e.RunSuspendedDetails)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
