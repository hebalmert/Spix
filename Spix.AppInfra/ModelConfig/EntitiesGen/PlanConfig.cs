using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesGen;

namespace Spix.AppInfra.ModelConfig.EntitiesGen;

public class PlanConfig : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.HasIndex(x => x.PlanId);
        builder.Property(x => x.PlanId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(x => new { x.CorporationId, x.PlanName });
        builder.Property(e => e.Price).HasPrecision(18, 2);
        //Evitar el borrado en cascada
        builder.HasOne(e => e.PlanCategory).WithMany(c => c.Plans).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Tax).WithMany(c => c.Plans).OnDelete(DeleteBehavior.Restrict);
    }
}