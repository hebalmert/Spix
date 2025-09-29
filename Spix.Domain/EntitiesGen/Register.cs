using Spix.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesGen;

public class Register
{
    [Key]
    public Guid RegisterId { get; set; }

    [Display(Name = "Contratos")]
    public int Contratos { get; set; }

    [Display(Name = "Solicitudes")]
    public int Solicitudes { get; set; }

    [Display(Name = "Compra")]
    public int RegPurchase { get; set; }

    [Display(Name = "Venta")]
    public int RegSells { get; set; }

    [Display(Name = "transferencia")]
    public int RegTransfer { get; set; }

    [Display(Name = "Cargue Inventario")]
    public int Cargue { get; set; }

    [Display(Name = "Egresos")]
    public int Egresos { get; set; }

    [Display(Name = "Adelantado")]
    public int Adelantado { get; set; }

    [Display(Name = "Pago Exonerado")]
    public int Exonerado { get; set; }

    [Display(Name = "Nota Cobro")]
    public int NotaCobro { get; set; }

    [Display(Name = "Factura")]
    public int Factura { get; set; }

    [Display(Name = "Pago Contratista")]
    public int PagoContratista { get; set; }

    //A que Corporacion Pertenece
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }
}