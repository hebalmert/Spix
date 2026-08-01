using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class RunSuspendedConfig : IEntityTypeConfiguration<RunSuspended>
{
    public void Configure(EntityTypeBuilder<RunSuspended> builder)
    {
        builder.HasKey(e => e.RunSuspendedId);
        builder.Property(e => e.RunSuspendedId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(e => e.DateUtc).HasColumnType("datetime2");
        builder.Property(e => e.UserByName).HasMaxLength(150);
        builder.HasIndex(e => new { e.CorporationId, e.YearNumber, e.MonthType }).IsUnique();

        builder.HasOne(e => e.Corporation)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
