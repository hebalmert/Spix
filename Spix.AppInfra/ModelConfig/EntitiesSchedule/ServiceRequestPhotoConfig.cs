using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesSchedule;

namespace Spix.AppInfra.ModelConfig.EntitiesSchedule;

public class ServiceRequestPhotoConfig : IEntityTypeConfiguration<ServiceRequestPhoto>
{
    public void Configure(EntityTypeBuilder<ServiceRequestPhoto> builder)
    {
        builder.HasKey(e => e.ServiceRequestPhotoId);
        builder.Property(e => e.ServiceRequestPhotoId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(e => e.DateCreated).HasColumnType("datetime2");

        //Se leen siempre las fotos de UNA solicitud
        builder.HasIndex(e => new { e.ServiceRequestId, e.PhotoType });

        //Al borrar la solicitud se van sus fotos: no tienen vida propia
        builder.HasOne(e => e.ServiceRequest)
            .WithMany(x => x.ServiceRequestPhotos)
            .HasForeignKey(e => e.ServiceRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Corporation)
            .WithMany()
            .HasForeignKey(e => e.CorporationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
