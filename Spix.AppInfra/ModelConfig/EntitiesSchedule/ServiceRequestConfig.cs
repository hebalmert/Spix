using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesSchedule;

namespace Spix.AppInfra.ModelConfig.EntitiesSchedule;

public class ServiceRequestConfig : IEntityTypeConfiguration<ServiceRequest>
{
    public void Configure(EntityTypeBuilder<ServiceRequest> builder)
    {
        builder.HasKey(e => e.ServiceRequestId);
        builder.Property(e => e.ServiceRequestId).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.HasIndex(e => new { e.CorporationId, e.RequestNumber }).IsUnique();

        builder.Property(e => e.UsuarioOwnerCompleted).HasMaxLength(256);

        builder.HasOne(e => e.Corporation)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ContractClient)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Technician)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ScheduleItem)
            .WithOne(e => e.ServiceRequest)
            .HasForeignKey<ScheduleItem>(e => e.ServiceRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Sell)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
