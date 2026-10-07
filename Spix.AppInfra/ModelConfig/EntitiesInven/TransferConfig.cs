using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesInven;

namespace Spix.AppInfra.ModelConfig.EntitiesInven;

public class TransferConfig : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.HasKey(e => e.TransferId);
        builder.Property(x => x.TransferId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.HasIndex(e => new { e.CorporationId, e.NroTransfer }).IsUnique();
        builder.Property(e => e.DateTransfer).HasColumnType("date");
        //Evitar el borrado en cascada
        builder.HasOne(e => e.User).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProductStorage>() .WithMany().HasForeignKey(e => e.FromProductStorageId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ProductStorage>().WithMany().HasForeignKey(e => e.ToProductStorageId).OnDelete(DeleteBehavior.Restrict);

        //Quien recibe: no se puede borrar el tecnico o el usuario si tiene traslados
        builder.HasOne(e => e.ReceivedByTechnician).WithMany().HasForeignKey(e => e.ReceivedByTechnicianId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ReceivedByUsuario).WithMany().HasForeignKey(e => e.ReceivedByUsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}