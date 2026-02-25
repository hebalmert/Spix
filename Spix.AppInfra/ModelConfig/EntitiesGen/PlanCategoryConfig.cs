using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesGen;

namespace Spix.AppInfra.ModelConfig.EntitiesGen;

public class PlanCategoryConfig : IEntityTypeConfiguration<PlanCategory>
{
    public void Configure(EntityTypeBuilder<PlanCategory> builder)
    {
        builder.HasKey(X => X.PlanCategoryId);
        builder.Property(x => x.PlanCategoryId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(x => new { x.PlanCategoryName, x.CorporationId }).IsUnique();
    }
}