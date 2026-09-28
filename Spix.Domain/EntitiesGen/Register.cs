using Spix.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using Spix.xLanguage.Resources;

namespace Spix.Domain.EntitiesGen;

public class Register
{
    [Key]
    public Guid RegisterId { get; set; }

    [Display(Name = "Contratos")]
    [Range(0, int.MaxValue, ErrorMessageResourceName = nameof(Resource.Validation_Range), ErrorMessageResourceType = typeof(Resource))]
    public int Contratos { get; set; }

    [Display(Name = "Solicitudes")]
    [Range(0, int.MaxValue, ErrorMessageResourceName = nameof(Resource.Validation_Range), ErrorMessageResourceType = typeof(Resource))]
    public int Solicitudes { get; set; }

    [Display(Name = "Compra")]
    [Range(0, int.MaxValue, ErrorMessageResourceName = nameof(Resource.Validation_Range), ErrorMessageResourceType = typeof(Resource))]
    public int RegPurchase { get; set; }

    [Display(Name = "Venta")]
    [Range(0, int.MaxValue, ErrorMessageResourceName = nameof(Resource.Validation_Range), ErrorMessageResourceType = typeof(Resource))]
    public int RegSells { get; set; }

    [Display(Name = "transferencia")]
    [Range(0, int.MaxValue, ErrorMessageResourceName = nameof(Resource.Validation_Range), ErrorMessageResourceType = typeof(Resource))]
    public int RegTransfer { get; set; }

    [Display(Name = "Cargue Inventario")]
    [Range(0, int.MaxValue, ErrorMessageResourceName = nameof(Resource.Validation_Range), ErrorMessageResourceType = typeof(Resource))]
    public int Cargue { get; set; }

    [Display(Name = "Egresos")]
    [Range(0, int.MaxValue, ErrorMessageResourceName = nameof(Resource.Validation_Range), ErrorMessageResourceType = typeof(Resource))]
    public int Egresos { get; set; }

    [Display(Name = "Adelantado")]
    [Range(0, int.MaxValue, ErrorMessageResourceName = nameof(Resource.Validation_Range), ErrorMessageResourceType = typeof(Resource))]
    public int Adelantado { get; set; }

    [Display(Name = "Pago Exonerado")]
    [Range(0, int.MaxValue, ErrorMessageResourceName = nameof(Resource.Validation_Range), ErrorMessageResourceType = typeof(Resource))]
    public int Exonerado { get; set; }

    [Display(Name = "Nota Cobro")]
    [Range(0, int.MaxValue, ErrorMessageResourceName = nameof(Resource.Validation_Range), ErrorMessageResourceType = typeof(Resource))]
    public int NotaCobro { get; set; }

    [Display(Name = "Factura")]
    [Range(0, int.MaxValue, ErrorMessageResourceName = nameof(Resource.Validation_Range), ErrorMessageResourceType = typeof(Resource))]
    public int Factura { get; set; }

    [Display(Name = "Pago Contratista")]
    [Range(0, int.MaxValue, ErrorMessageResourceName = nameof(Resource.Validation_Range), ErrorMessageResourceType = typeof(Resource))]
    public int PagoContratista { get; set; }

    //A que Corporacion Pertenece
    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }
}