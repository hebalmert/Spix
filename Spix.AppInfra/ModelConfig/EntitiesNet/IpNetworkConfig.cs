using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesNet;

namespace Spix.AppInfra.ModelConfig.EntitiesNet;

public class IpNetworkConfig : IEntityTypeConfiguration<IpNetwork>
{
    public void Configure(EntityTypeBuilder<IpNetwork> builder)
    {
        builder.HasKey(e => e.IpNetworkId);
        builder.Property(x => x.IpNetworkId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.Ip, e.CorporationId }).IsUnique();

        //Orden numerico de la IP: la clave la calcula la entidad y el indice sirve el listado paginado
        builder.HasIndex(e => new { e.CorporationId, e.IpSort });
    }
}