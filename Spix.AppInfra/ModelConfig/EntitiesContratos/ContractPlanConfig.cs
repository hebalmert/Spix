using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractPlanConfig : IEntityTypeConfiguration<ContractPlan>
{
    public void Configure(EntityTypeBuilder<ContractPlan> builder)
    {
        builder.HasKey(e => e.ContractPlanId);
        builder.Property(x => x.ContractPlanId).HasDefaultValueSql("NEWSEQUENTIALID()");
        //UN plan por contrato. El indice estaba sobre el par, que solo impide repetir el
        //mismo plan: un contrato podia terminar con dos planes distintos y el queue padre,
        //que se arma por (Plan, Servidor), no sabria cual usar.
        builder.HasIndex(e => e.ContractClientId).IsUnique();

        //Evitar el borrado en cascada
        builder.HasOne(e => e.ContractClient).WithMany(c => c.ContractPlans).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Plan).WithMany(c => c.ContractPlans).OnDelete(DeleteBehavior.Restrict);
    }
}