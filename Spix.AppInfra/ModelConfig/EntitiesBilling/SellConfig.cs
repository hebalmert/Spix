using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesBilling;

namespace Spix.AppInfra.ModelConfig.EntitiesBilling;

public class SellConfig : IEntityTypeConfiguration<Sell>
{
    public void Configure(EntityTypeBuilder<Sell> builder)
    {
        builder.HasKey(e => e.SellId);
        builder.Property(e => e.SellId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.InvoiceNumber }).IsUnique();
        //El listado y el tablero siempre van por corporacion y fecha: sin este indice
        //la consulta recorre toda la tabla y el servidor la cancela por costo.
        builder.HasIndex(e => new { e.CorporationId, e.DateSell });
        //El portal del cliente pide SIEMPRE sus propias facturas por fecha: con este
        //indice la consulta entra derecho a las de el, sin recorrer las de la corporacion.
        builder.HasIndex(e => new { e.CorporationId, e.ClientId, e.DateSell });
        builder.Property(e => e.DateSell).HasColumnType("date");
        builder.Property(e => e.DateCancelled).HasColumnType("date");
        builder.Property(e => e.DatePaid).HasColumnType("date");

        builder.HasOne(e => e.Corporation).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ContractClient).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Client).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.BillingNote).WithMany(e => e.Sells).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.BillingNoteOne).WithMany(e => e.Sells).OnDelete(DeleteBehavior.Restrict);
    }
}
