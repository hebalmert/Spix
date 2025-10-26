using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Core.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractPlanConfig : IEntityTypeConfiguration<ContractPlan>
{
    public void Configure(EntityTypeBuilder<ContractPlan> builder)
    {
        builder.HasKey(e => e.ContractPlanId);
        builder.HasIndex(e => new { e.ContractClientId, e.PlanId }).IsUnique();

        //Evitar el borrado en cascada
        builder.HasOne(e => e.ContractClient).WithMany(c => c.ContractPlans).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Plan).WithMany(c => c.ContractPlans).OnDelete(DeleteBehavior.Restrict);
    }
}