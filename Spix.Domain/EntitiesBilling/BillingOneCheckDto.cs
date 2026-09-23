namespace Spix.Domain.EntitiesBilling;

//La revision de una nota individual: que se le va a cobrar a ESE cliente y que le falta
//al contrato para poder cobrarle. No cambia nada, solo mira.
public class BillingOneCheckDto
{
    public long ControlContrato { get; set; }

    public string ClientFullName { get; set; } = null!;

    public string? ZoneName { get; set; }

    //Un contrato que no esta activo no se factura
    public bool IsActive { get; set; }

    //Ya tiene factura o cuenta por cobrar viva del periodo
    public bool AlreadyBilled { get; set; }

    //Lo que le falta al contrato: sin esto no se le puede cobrar ni dar servicio
    public bool HasPlan { get; set; }

    public bool HasIp { get; set; }

    public bool HasMac { get; set; }

    public bool HasServer { get; set; }

    public bool HasNode { get; set; }

    public bool HasQueue { get; set; }

    public bool HasBinding { get; set; }

    //Lo que se le va a cobrar
    public string? PlanName { get; set; }

    public decimal PlanPrice { get; set; }

    public List<BillingOneLineDto> Services { get; set; } = new();

    public decimal ServicesTotal { get; set; }

    public decimal Total { get; set; }

    //Lo que se cruza: el adelanto que ya pago y la exoneracion del mes
    public decimal PrePayment { get; set; }

    public decimal Exonerated { get; set; }

    public decimal Balance { get; set; }

    //El mismo mes no puede tener exoneracion y pago adelantado
    public bool PrePaymentAndExonerated { get; set; }
}

//Un renglon de lo que se le va a cobrar
public class BillingOneLineDto
{
    public string Concept { get; set; } = null!;

    public decimal Price { get; set; }
}
