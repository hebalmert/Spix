using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesEmails;

namespace Spix.AppInfra.ModelConfig.EntitiesEmails;

public class EmailProviderSettingConfig : IEntityTypeConfiguration<EmailProviderSetting>
{
    public void Configure(EntityTypeBuilder<EmailProviderSetting> builder)
    {
        builder.HasKey(e => e.EmailProviderSettingId);
        builder.Property(e => e.EmailProviderSettingId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.ProviderType, e.Name }).IsUnique();
        builder.Property(e => e.DateCreated).HasColumnType("date");

        builder.HasOne(e => e.Corporation)
            .WithMany()
            .HasForeignKey(e => e.CorporationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
