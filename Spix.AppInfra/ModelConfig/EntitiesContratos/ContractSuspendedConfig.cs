using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractSuspendedConfig : IEntityTypeConfiguration<ContractSuspended>
{
    public void Configure(EntityTypeBuilder<ContractSuspended> builder)
    {
        builder.HasKey(e => e.ContractSuspendedId);
        builder.Property(e => e.ContractSuspendedId).HasDefaultValueSql("NEWSEQUENTIALID()");

        //Se busca por contrato (para cerrar la suspension abierta) y por fecha (para los reportes)
        builder.HasIndex(e => new { e.ContractClientId, e.DateReactivated });
        builder.HasIndex(e => new { e.CorporationId, e.DateSuspended });

        //Los que ya pagaron y esperan reactivacion: es la consulta del modulo
        builder.HasIndex(e => new { e.CorporationId, e.PaymentReceived, e.DateReactivated });
        builder.Property(e => e.DatePaymentReceived).HasColumnType("date");

        builder.HasOne(e => e.Corporation)
            .WithMany()
            .HasForeignKey(e => e.CorporationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ContractClient)
            .WithMany()
            .HasForeignKey(e => e.ContractClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Client)
            .WithMany()
            .HasForeignKey(e => e.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.RunSuspended)
            .WithMany()
            .HasForeignKey(e => e.RunSuspendedId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
