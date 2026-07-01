using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesMK;

namespace Spix.AppInfra.ModelConfig.EntitiesMK;

public class QueueParentConfig : IEntityTypeConfiguration<QueueParent>
{
    public void Configure(EntityTypeBuilder<QueueParent> builder)
    {
        builder.HasKey(e => e.QueueParentId);
        builder.Property(e => e.QueueParentId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.ParentName, e.ServerId}).IsUnique();
        //Evitar el borrado en cascada
        builder.HasOne(e => e.Server).WithMany(c => c.QueueParents).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Plan).WithMany(c => c.QueueParents).OnDelete(DeleteBehavior.Restrict);
    }
}
