using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesGen;

namespace Spix.AppInfra.ModelConfig.EntitiesGen;

public class RegisterConfig : IEntityTypeConfiguration<Register>
{
    public void Configure(EntityTypeBuilder<Register> builder)
    {
        builder.HasKey(e => e.RegisterId);
        builder.Property(x => x.RegisterId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.Contratos }).IsUnique();
        builder.HasIndex(e => new { e.CorporationId, e.Solicitudes }).IsUnique();
        builder.HasIndex(e => new { e.CorporationId, e.Cargue }).IsUnique();
        builder.HasIndex(e => new { e.CorporationId, e.Egresos }).IsUnique();
        builder.HasIndex(e => new { e.CorporationId, e.Adelantado }).IsUnique();
        builder.HasIndex(e => new { e.CorporationId, e.Exonerado }).IsUnique();
        builder.HasIndex(e => new { e.CorporationId, e.NotaCobro }).IsUnique();
        builder.HasIndex(e => new { e.CorporationId, e.Factura }).IsUnique();
        builder.HasIndex(e => new { e.CorporationId, e.PagoContratista }).IsUnique();
    }
}