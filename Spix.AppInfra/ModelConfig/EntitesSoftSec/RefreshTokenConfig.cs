using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitesSoftSec;

namespace Spix.AppInfra.ModelConfig.EntitesSoftSec;

public class RefreshTokenConfig : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Token).IsRequired().HasMaxLength(4000);

        builder.Property(rt => rt.Expiration).IsRequired();

        builder.Property(rt => rt.IsRevoked).IsRequired();

        // RELACIÓN CORRECTA
        builder.HasOne(rt => rt.User).WithMany() // si luego quieres lista de tokens, aquí va .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
