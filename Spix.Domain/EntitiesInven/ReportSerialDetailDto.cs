using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesInven;

//Un serial, uno por fila. Es el detalle del reporte que hoy solo da totales.
//
//Los [Display] son los encabezados del Excel: el exportador los lee por reflexion, asi que
//la hoja sale rotulada sin armar nada aparte.
public class ReportSerialDetailDto
{
    [Display(Name = "MAC / Serial")]
    public string MacWlan { get; set; } = string.Empty;

    [Display(Name = "Producto")]
    public string ProductName { get; set; } = string.Empty;

    [Display(Name = "Bodega")]
    public string StorageName { get; set; } = string.Empty;

    [Display(Name = "Estado")]
    public string StatusName { get; set; } = string.Empty;

    //Solo los instalados tienen cliente y contrato
    [Display(Name = "Cliente")]
    public string ClientName { get; set; } = string.Empty;

    [Display(Name = "Contrato")]
    public string ContractNumber { get; set; } = string.Empty;

    [Display(Name = "Cargue")]
    public string CargueNumber { get; set; } = string.Empty;

    [Display(Name = "Fecha cargue")]
    public DateTime? DateCargue { get; set; }

    [Display(Name = "Observacion")]
    public string Comment { get; set; } = string.Empty;
}
