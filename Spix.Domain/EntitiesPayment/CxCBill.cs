using Spix.Domain.Entities;
using Spix.Domain.EntitiesBilling;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.EnumTypes;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesPayment;

public class CxCBill
{
    [Key]
    public Guid CxCBillId { get; set; }

    public DateTime DateNote { get; set; }

    //El periodo que cobra esta nota. Se guarda aqui (no solo en la nota general) para que la
    //base pueda impedir que un contrato quede facturado dos veces el mismo mes, aunque el
    //lanzamiento por lotes se caiga a mitad y se reintente.
    public int YearNumber { get; set; }

    public MonthType MonthType { get; set; }

    [MaxLength(25)]
    public string? CollectionNote { get; set; }

    public Guid ClientId { get; set; }

    public Guid ContractClientId { get; set; }

    [MaxLength(250)]
    public string Description { get; set; } = null!;

    public decimal Total { get; set; }

    public decimal Balance { get; set; }

    public decimal TotalPayment => CxCBillDetails == null ? 0 : CxCBillDetails.Sum(x => x.Payment);

    public decimal TotalDiscount => CxCBillDetails == null ? 0 : CxCBillDetails.Sum(x => x.Discount);

    public decimal TotalPaid => CxCBillDetails == null ? 0 : CxCBillDetails.Sum(x => x.TotalPayments);

    public Guid SellId { get; set; }

    public Guid? BillingNoteOneId { get; set; }

    public bool Paid { get; set; }

    public DateTime? DatePaid { get; set; }

    //Anulacion del CxCBill: no puede recibir pagos ni generar nota de credito.
    //El contrato puede facturarse nuevamente para el periodo anulado.
    public bool Cancelled { get; set; }

    public DateTime? DateCancelled { get; set; }

    [MaxLength(250)]
    public string? DescriptionCancelled { get; set; }

    public string? UsuarioOwnerCancelled { get; set; }

    public Guid? UserIdCancelled { get; set; }

    //Fin Auditoria de Control Anulacion 


    public int CorporationId { get; set; }

    public string? UsuarioOwner { get; set; }

    public Guid? UserId { get; set; }

    public Corporation? Corporation { get; set; }

    public Client? Client { get; set; }

    public ContractClient? ContractClient { get; set; }

    public Sell? Sell { get; set; }

    public BillingNoteOne? BillingNoteOne { get; set; }

    public ICollection<CxCBillDetail>? CxCBillDetails { get; set; }

    public ICollection<PrePayment>? PrePayments { get; set; }

    public ICollection<ContractExonerated>? ContractExonerateds { get; set; }

    public ICollection<ContractorAccountPayable>? ContractorAccountPayables { get; set; }

    public ICollection<RunSuspendedDetail>? RunSuspendedDetails { get; set; }
}
