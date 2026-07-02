using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesSchedule;

namespace Spix.AppInfra.ModelConfig.EntitiesSchedule;

public class ServiceRequestDetailConfig : IEntityTypeConfiguration<ServiceRequestDetail>
{
    public void Configure(EntityTypeBuilder<ServiceRequestDetail> builder)
    {
        builder.HasKey(e => e.ServiceRequestDetailId);
        builder.Property(e => e.ServiceRequestDetailId).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.HasOne(e => e.ServiceRequest)
            .WithMany(e => e.ServiceRequestDetails)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ServiceCategory)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ServiceClient)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
