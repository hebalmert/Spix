using Spix.Domain.Entities;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesSchedule;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesBilling;

public class SellDetail
{
    [Key]
    public Guid SellDetailId { get; set; }

    public Guid SellId { get; set; }

    [MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(50)]
    public string? Origin { get; set; }

    [MaxLength(250)]
    public string? Concept { get; set; }

    public decimal Quantity { get; set; }

    public Guid? TaxId { get; set; }

    public decimal TaxRate { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal Price { get; set; }

    public decimal TotalUnitPrice => UnitPrice * Quantity;

    public decimal TotalTaxAmount => TaxAmount * Quantity;

    public decimal TotalPrice => Price * Quantity;

    public Guid? ServiceRequestId { get; set; }

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public Sell? Sell { get; set; }

    public Tax? Tax { get; set; }

    public ServiceRequest? ServiceRequest { get; set; }
}
