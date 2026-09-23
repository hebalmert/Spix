using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesPayment;

namespace Spix.AppInfra.ModelConfig.EntitiesPayment;

public class CxCBillDetailConfig : IEntityTypeConfiguration<CxCBillDetail>
{
    public void Configure(EntityTypeBuilder<CxCBillDetail> builder)
    {
        builder.HasKey(e => e.CxCBillDetailId);
        builder.Property(e => e.CxCBillDetailId).HasDefaultValueSql("NEWSEQUENTIALID()");
        //El cruce con el tecnico entra por aqui: corporacion, quien recibio y el periodo
        builder.HasIndex(e => new { e.CorporationId, e.UserId, e.DatePayment });
        builder.Property(e => e.DatePayment).HasColumnType("date");
        builder.Property(e => e.Debt).HasPrecision(18, 2);
        builder.Property(e => e.Payment).HasPrecision(18, 2);
        builder.Property(e => e.Discount).HasPrecision(18, 2);
        builder.Property(e => e.Balance).HasPrecision(18, 2);

        builder.HasOne(e => e.Corporation).WithMany().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.CxCBill).WithMany(e => e.CxCBillDetails).OnDelete(DeleteBehavior.Restrict);
    }
}
