using Spix.Domain.Entities;
using Spix.Domain.EntitiesSchedule;
using Spix.DomainLogic.EnumTypes;
using System.ComponentModel.DataAnnotations;

namespace Spix.Domain.EntitiesPayment;

//Una linea de lo que el cliente esta pagando por adelantado: el plan del mes, o un servicio
//de una solicitud tecnica. Los valores quedan congelados al momento del pago: es plata recibida.
public class PrePaymentDetail
{
    [Key]
    public Guid PrePaymentDetailId { get; set; }

    [Required]
    public Guid PrePaymentId { get; set; }

    //De donde sale la linea: el plan del contrato o un servicio
    public PrePaymentLineType LineType { get; set; }

    [MaxLength(250)]
    public string Concept { get; set; } = null!;

    //Solo para las lineas de plan
    public Guid? PlanId { get; set; }

    //Solo para las lineas de servicio: la solicitud queda reservada para este pago
    public Guid? ServiceRequestId { get; set; }

    public Guid? ServiceRequestDetailId { get; set; }

    public decimal TaxRate { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal PriceWithTax { get; set; }

    public int CorporationId { get; set; }

    public Corporation? Corporation { get; set; }

    public PrePayment? PrePayment { get; set; }

    public ServiceRequest? ServiceRequest { get; set; }
}
