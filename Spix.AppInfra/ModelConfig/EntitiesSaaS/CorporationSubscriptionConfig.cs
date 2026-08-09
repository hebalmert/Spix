using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesSaaS;

namespace Spix.AppInfra.ModelConfig.EntitiesSaaS;

public class CorporationSubscriptionConfig : IEntityTypeConfiguration<CorporationSubscription>
{
    public void Configure(EntityTypeBuilder<CorporationSubscription> builder)
    {
        builder.HasKey(x => x.CorporationSubscriptionId);
        builder.Property(x => x.CorporationSubscriptionId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(x => x.ExternalReference).IsUnique();
        builder.HasIndex(x => new { x.CorporationId, x.Status });
        builder.Property(x => x.DateCreatedUtc).HasColumnType("datetime2");

        builder.HasOne(x => x.Corporation)
            .WithMany()
            .HasForeignKey(x => x.CorporationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SoftPlan)
            .WithMany()
            .HasForeignKey(x => x.SoftPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
