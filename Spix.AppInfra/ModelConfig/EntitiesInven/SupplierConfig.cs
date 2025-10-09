﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spix.Domain.EntitiesInven;

namespace Spix.Infrastructure.ModelConfig.EntitiesInven;

public class SupplierConfig : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.HasKey(e => e.SupplierId);
        builder.HasIndex(e => new { e.CorporationId, e.Name }).IsUnique();
        builder.HasIndex(e => new { e.CorporationId, e.Document, e.DocumentTypeId }).IsUnique();
        //Evitar el borrado en cascada
        builder.HasOne(e => e.State).WithMany(c => c.Supplier).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.City).WithMany(c => c.Supplier).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.DocumentType).WithMany(c => c.Suppliers).OnDelete(DeleteBehavior.Restrict);
    }
}