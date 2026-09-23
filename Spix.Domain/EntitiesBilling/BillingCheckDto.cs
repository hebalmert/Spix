namespace Spix.Domain.EntitiesBilling;

//Un contrato activo revisado antes de lanzar las notas: dice que le falta para poder cobrarle.
//Se arma en UNA sola consulta para todos los contratos, no uno por uno.
public class BillingCheckDto
{
    public Guid ContractClientId { get; set; }

    public long ControlContrato { get; set; }

    public string ClientFullName { get; set; } = null!;

    public string? ZoneName { get; set; }

    //Sin plan no se puede cobrar: es lo unico que detiene la nota de ese contrato
    public bool HasPlan { get; set; }

    //Lo demas es configuracion del servicio: se avisa, pero no impide cobrar
    public bool HasServer { get; set; }

    public bool HasIp { get; set; }

    public bool HasMac { get; set; }

    public bool HasNode { get; set; }

    public bool HasQueue { get; set; }

    public bool HasBinding { get; set; }

    //Lo que ya tiene nota de cobro para el periodo que se va a lanzar
    public bool AlreadyBilled { get; set; }
}
