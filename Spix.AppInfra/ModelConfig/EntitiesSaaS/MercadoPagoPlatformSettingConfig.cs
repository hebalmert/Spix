using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesSaaS;

namespace Spix.AppInfra.ModelConfig.EntitiesSaaS;

public class MercadoPagoPlatformSettingConfig : IEntityTypeConfiguration<MercadoPagoPlatformSetting>
{
    public void Configure(EntityTypeBuilder<MercadoPagoPlatformSetting> builder)
    {
        builder.HasKey(x => x.MercadoPagoPlatformSettingId);
        builder.Property(x => x.MercadoPagoPlatformSettingId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(x => x.DateModifiedUtc).HasColumnType("datetime2");
    }
}
