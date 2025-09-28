using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesData;

namespace Spix.AppInfra.ModelConfig.EntitiesData;

public class ChannelConfig : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.HasKey(e => e.ChannelId);
        builder.HasIndex(e => e.ChannelName).IsUnique();
    }
}