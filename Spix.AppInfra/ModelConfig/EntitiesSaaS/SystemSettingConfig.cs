using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesSaaS;

namespace Spix.AppInfra.ModelConfig.EntitiesSaaS;

public class SystemSettingConfig : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.HasKey(e => e.SystemSettingId);

        builder.HasIndex(e => e.Key)
            .IsUnique();
    }
}
