using Spix.Domain.Entities;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.EnumTypes;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesPayment;

//Bitacora del dinero: un renglon por cada movimiento, venga del modulo que venga
//(pagos adelantados, exoneraciones, notas de cobro y recaudo).
//Solo se escribe: nunca se edita ni se borra. Lo delicado no se borra, se anula, y esa
//anulacion es otro renglon.
//
//Va aparte de ContractAudit a proposito: aquella cuenta que le paso al SERVICIO del cliente;
//esta cuenta quien movio PLATA, cuando y cuanto. Ademas crece mucho mas rapido.
public class PaymentAudit
{
    [Key]
    public Guid PaymentAuditId { get; set; }

    public DateTime DateEvent { get; set; }

    public PaymentEventType EventType { get; set; }

    //Sobre que contrato y cliente fue el movimiento
    public Guid? ContractClientId { get; set; }

    public Guid? ClientId { get; set; }

    //El documento que lo origino (el adelanto, la nota de cobro, el pago) y de que tipo es,
    //para poder ir a verlo sin adivinar.
    public Guid? ReferenceId { get; set; }

    [MaxLength(40)]
    public string? ReferenceType { get; set; }

    //Cuanto se movio
    public decimal Amount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal Total { get; set; }

    //La foto del momento: lo que decia el registro, para que un borrado no se lleve la evidencia
    [MaxLength(1000)]
    public string? Detail { get; set; }

    //Quien lo hizo y desde donde
    public Guid? UserId { get; set; }

    [MaxLength(150)]
    public string? UserByName { get; set; }

    [MaxLength(64)]
    public string? SourceIp { get; set; }

    [MaxLength(512)]
    public string? UserAgent { get; set; }

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public ContractClient? ContractClient { get; set; }

    public Client? Client { get; set; }
}
