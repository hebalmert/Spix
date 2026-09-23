namespace Spix.Domain.EntitiesPayment;

//Un cobro recibido por alguien, como se ve en el cruce con el tecnico
public class TechnicianCollectionDto
{
    public DateTime DatePayment { get; set; }

    public string? CollectionNote { get; set; }

    public long ControlContrato { get; set; }

    public string? ClientFullName { get; set; }

    public string? PaymentMode { get; set; }

    public decimal Payment { get; set; }

    public decimal Discount { get; set; }
}

//El resumen de lo que recogio en el periodo: es lo que hay que recibirle
public class TechnicianCollectionSummaryDto
{
    public int Collections { get; set; }

    public decimal Total { get; set; }

    //Separado por modo: al tecnico se le recibe el efectivo, lo demas ya entro al banco
    public decimal Cash { get; set; }

    public decimal Card { get; set; }

    public decimal Transfer { get; set; }
}
