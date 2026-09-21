using Spix.Domain.Entities;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesGen;
using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.EnumTypes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spix.Domain.EntitiesPayment;

//Exoneracion de un contrato por un MES concreto (ano + mes): el cliente no paga ese mes.
//Cuando se generan las notas de cobro, el sistema mira si el contrato esta exonerado para
//ese periodo; si lo esta, no le genera la nota y de una vez cierra la exoneracion.
//
//La fila NO se borra: se cierra con DateEnded y queda como historia, igual que en
//ContractSuspended. Los datos del cliente, del contrato y del plan quedan guardados como
//columnas propias (la foto del momento), para que un cambio de precio o de direccion no
//altere lo que se decidio ese dia.
//
//Distinta de ContractExempt, que es la exoneracion FIJA, sin vencimiento.
public class ContractExonerated
{
    [Key]
    public Guid ContractExoneratedId { get; set; }

    public DateTime DateExonerated { get; set; }

    [MaxLength(25)]
    public string? ExoneratedControl { get; set; }

    public Guid ClientId { get; set; }

    public Guid ContractClientId { get; set; }

    public Guid PlanId { get; set; }

    //Periodo exonerado: un mes de un ano
    public int YearNumber { get; set; }

    public MonthType MonthType { get; set; }

    public decimal TaxRate { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal PriceWithTax { get; set; }

    public bool Billed { get; set; }

    public DateTime? DateBilled { get; set; }

    public Guid? CxCBillId { get; set; }

    //===== Cierre de la exoneracion =====
    //Nulo = sigue vigente. Se cierra cuando pasa el mes y la facturacion la consume,
    //o cuando alguien la retira a mano.
    public DateTime? DateEnded { get; set; }

    [MaxLength(200)]
    public string? Motivo { get; set; }

    //===== Foto del momento de la exoneracion =====
    //Columnas propias a proposito: los reportes no tienen que ir a las otras tablas, y si
    //manana el plan sube de precio o el cliente cambia de direccion, el registro sigue
    //mostrando lo que habia el dia que se exonero.

    public long ControlContrato { get; set; }

    [MaxLength(200)]
    public string? ClientName { get; set; }

    [MaxLength(50)]
    public string? ClientDocument { get; set; }

    [MaxLength(200)]
    public string? ContractAddress { get; set; }

    [MaxLength(50)]
    public string? ContractPhone { get; set; }

    [MaxLength(100)]
    public string? CityName { get; set; }

    [MaxLength(100)]
    public string? ZoneName { get; set; }

    [MaxLength(100)]
    public string? PlanName { get; set; }

    [NotMapped]
    public decimal? TaxTotal => TaxRate == 0 ? 0 : PriceWithTax - UnitPrice;

    [NotMapped]
    public decimal? TotalUnitPrice => UnitPrice;

    [NotMapped]
    public decimal? TotalWithTax => PriceWithTax;

    public int CorporationId { get; set; }

    //Quien exonero y quien cerro la exoneracion
    public Guid? UserId { get; set; }

    [MaxLength(150)]
    public string? UserByName { get; set; }

    public Guid? UserIdEnded { get; set; }

    [MaxLength(150)]
    public string? UserByNameEnded { get; set; }

    public Corporation? Corporation { get; set; }

    public Client? Client { get; set; }

    public ContractClient? ContractClient { get; set; }

    public Plan? Plan { get; set; }

    public CxCBill? CxCBill { get; set; }
}
