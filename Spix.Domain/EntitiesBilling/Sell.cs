using Spix.Domain.Entities;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesOper;
using Spix.Domain.EntitiesPayment;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesBilling;

public class Sell
{
    [Key]
    public Guid SellId { get; set; }

    public DateTime DateSell { get; set; }

    [MaxLength(25)]
    public string? InvoiceNumber { get; set; }

    public Guid ContractClientId { get; set; }

    public long ControlContrato { get; set; }

    public Guid ClientId { get; set; }

    [MaxLength(150)]
    public string ClientFullName { get; set; } = null!;

    public Guid? DocumentTypeId { get; set; }

    [MaxLength(50)]
    public string? DocumentTypeName { get; set; }

    [MaxLength(25)]
    public string? Identification { get; set; }

    [MaxLength(25)]
    public string? PhoneNumber { get; set; }

    [MaxLength(256)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? ZoneName { get; set; }

    public decimal SubTotal => SellDetails == null ? 0 : SellDetails.Sum(x => x.TotalUnitPrice);

    public decimal TotalTax => SellDetails == null ? 0 : SellDetails.Sum(x => x.TotalTaxAmount);

    public decimal Total => SellDetails == null ? 0 : SellDetails.Sum(x => x.TotalPrice);

    public bool Cancelled { get; set; }

    public DateTime? DateCancelled { get; set; }

    public bool Printed { get; set; }

    public bool Paid { get; set; }

    public DateTime? DatePaid { get; set; }

    public Guid? BillingNoteId { get; set; }

    public Guid? BillingNoteOneId { get; set; }

    public int CorporationId { get; set; }

    public string? UsuarioOwner { get; set; }

    public Guid? UserId { get; set; }

    public Corporation? Corporation { get; set; }

    public ContractClient? ContractClient { get; set; }

    public Client? Client { get; set; }

    public BillingNote? BillingNote { get; set; }

    public BillingNoteOne? BillingNoteOne { get; set; }

    public ICollection<SellDetail>? SellDetails { get; set; }

    public ICollection<CxCBill>? CxCBills { get; set; }
}
