using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesContratos;

namespace Spix.AppInfra.ModelConfig.EntitiesContratos;

public class ContractIDPicConfig : IEntityTypeConfiguration<ContractIDPic>
{
    public void Configure(EntityTypeBuilder<ContractIDPic> builder)
    {
        builder.HasKey(e => e.ContractIDPicId);
        builder.Property(x => x.ContractIDPicId).HasDefaultValueSql("NEWSEQUENTIALID()");
        //Evitar el borrado en cascada
        builder.HasOne(e => e.ContractClient).WithOne(c => c.ContractIDPic).HasForeignKey<ContractIDPic>(e => e.ContractClientId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
