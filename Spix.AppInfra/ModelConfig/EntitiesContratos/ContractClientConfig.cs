using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractClientConfig : IEntityTypeConfiguration<ContractClient>
{
    public void Configure(EntityTypeBuilder<ContractClient> builder)
    {
        builder.HasKey(e => e.ContractClientId);
        builder.HasIndex(e => new { e.CorporationId, e.ControlContrato }).IsUnique();
        builder.Property(e => e.DateCreado).HasColumnType("date");
        builder.Property(e => e.Impuesto).HasPrecision(18, 2);
        builder.Property(e => e.Price).HasPrecision(18, 2);
        //Evitar el borrado en cascada
        builder.HasOne(e => e.Contractor).WithMany(c => c.ContractClients).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Client).WithMany(c => c.ContractClients).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Zone).WithMany(c => c.ContractClients).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ServiceCategory).WithMany(c => c.ContractClients).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ServiceClient).WithMany(c => c.ContractClients).OnDelete(DeleteBehavior.Restrict);
    }
}