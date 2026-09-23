using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesPayment;

namespace Spix.AppInfra.ModelConfig.EntitiesPayment;

public class CxCContractorDetailConfig : IEntityTypeConfiguration<CxCContractorDetail>
{
    public void Configure(EntityTypeBuilder<CxCContractorDetail> builder)
    {
        builder.HasKey(e => e.CxCContractorDetailId);
        builder.Property(e => e.CxCContractorDetailId).HasDefaultValueSql("NEWSEQUENTIALID()");
        builder.Property(e => e.DatePayment).HasColumnType("date");
        builder.Property(e => e.Debt).HasPrecision(18, 2);
        builder.Property(e => e.Payment).HasPrecision(18, 2);
        builder.Property(e => e.Balance).HasPrecision(18, 2);

        builder.HasOne(e => e.Corporation).WithMany().OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CxCContractor)
            .WithMany(e => e.CxCContractorDetails)
            .HasForeignKey(e => e.CxCContractorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
