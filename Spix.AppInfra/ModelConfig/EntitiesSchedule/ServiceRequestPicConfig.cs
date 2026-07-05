using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesSchedule;

namespace Spix.AppInfra.ModelConfig.EntitiesSchedule;

public class ServiceRequestPicConfig : IEntityTypeConfiguration<ServiceRequestPic>
{
    public void Configure(EntityTypeBuilder<ServiceRequestPic> builder)
    {
        builder.HasKey(e => e.ServiceRequestPicId);
        builder.Property(e => e.ServiceRequestPicId).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.HasOne(e => e.Corporation)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ServiceRequest)
            .WithOne(e => e.ServiceRequestPic)
            .HasForeignKey<ServiceRequestPic>(e => e.ServiceRequestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
