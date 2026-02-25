using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesGen;

namespace Spix.AppInfra.ModelConfig.EntitiesGen;

public class MarkModelConfig : IEntityTypeConfiguration<MarkModel>
{
    public void Configure(EntityTypeBuilder<MarkModel> builder)
    {
        builder.HasKey(e => e.MarkModelId);
        builder.Property(x => x.MarkModelId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(x => new { x.MarkModelName, x.CorporationId, x.MarkId }).IsUnique();
        //Evitar el borrado en cascada
        builder.HasOne(e => e.Mark).WithMany(c => c.MarkModels).OnDelete(DeleteBehavior.Restrict);
    }
}