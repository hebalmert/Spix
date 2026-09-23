namespace Spix.Domain.EntitiesPayment;

//Una comision pendiente de agrupar, como se ve al armar la cuenta del contratista
public class ContractorPendingDto
{
    public Guid ContractorAccountPayableId { get; set; }

    public DateTime DateCreated { get; set; }

    public long ControlContrato { get; set; }

    public string? ClientFullName { get; set; }

    public string? CollectionNote { get; set; }

    //Lo que se le recibio al cliente, el porcentaje pactado y la comision
    public decimal BaseAmount { get; set; }

    public decimal Rate { get; set; }

    public decimal Total { get; set; }
}

//Lo que se manda para armar la cuenta del contratista
public class CxCContractorCreateDto
{
    public Guid ContractorId { get; set; }

    //Vacio = todas las comisiones pendientes de ese contratista
    public List<Guid> ContractorAccountPayableIds { get; set; } = new();
}

//Lo que se manda para pagarle, completo o por partes
public class CxCContractorPaymentDto
{
    public Guid CxCContractorId { get; set; }

    public decimal Payment { get; set; }

    public string PaymentMode { get; set; } = "Cash";

    public string? Reference { get; set; }

    public string? Detail { get; set; }
}

//Los numeros del tablero de cuentas por pagar a contratistas
public class CxCContractorSummaryDto
{
    //Comisiones causadas que todavia no estan en ninguna cuenta
    public int Pending { get; set; }

    public decimal PendingTotal { get; set; }

    //Cuentas abiertas y lo que falta por pagarles
    public int OpenNotes { get; set; }

    public decimal OpenBalance { get; set; }
}

//Un abono de la cuenta, como se ve en su listado
public class CxCContractorPaymentItemDto
{
    public DateTime DatePayment { get; set; }

    public string? PaymentMode { get; set; }

    public string? Reference { get; set; }

    public string? Detail { get; set; }

    public decimal Payment { get; set; }

    public decimal Balance { get; set; }

    public string? UsuarioOwner { get; set; }
}
