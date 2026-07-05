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
        builder.Property(e => e.TaxRate).HasPrecision(5, 2);
        builder.Property(e => e.Price).HasPrecision(18, 2);
        builder.Property(e => e.TaxAmount).HasPrecision(18, 2);

        builder.HasOne(e => e.ServiceRequest)
            .WithMany(e => e.ServiceRequestDetails)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.ServiceCategory)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ServiceClient)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Tax)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.SellDetail)
            .WithMany()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
