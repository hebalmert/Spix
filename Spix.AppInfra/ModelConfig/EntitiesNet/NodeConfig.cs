using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesNet;

namespace Spix.AppInfra.ModelConfig.EntitiesNet;

public class NodeConfig : IEntityTypeConfiguration<Node>
{
    public void Configure(EntityTypeBuilder<Node> builder)
    {
        builder.HasKey(e => e.NodeId);
        builder.Property(x => x.NodeId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.NodesName, e.CorporationId, e.OperationId }).IsUnique();
        //Evitar el borrado en cascada
        builder.HasOne(e => e.IpNetwork).WithMany(c => c.Nodes).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Operation).WithMany(c => c.Nodes).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Mark).WithMany(c => c.Nodes).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.MarkModel).WithMany(c => c.Nodes).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Zone).WithMany(c => c.Nodes).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.FrecuencyType).WithMany(c => c.Nodes).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Frecuency).WithMany(c => c.Nodes).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Channel).WithMany(c => c.Nodes).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Security).WithMany(c => c.Nodes).OnDelete(DeleteBehavior.Restrict);
    }
}