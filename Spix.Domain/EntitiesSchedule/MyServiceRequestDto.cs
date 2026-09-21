namespace Spix.Domain.EntitiesSchedule;

//Lo unico que el CLIENTE manda para pedir una visita. Va aparte de ServiceRequestDto a
//proposito: ese es el de la oficina y el tecnico, y lleva tecnico, fecha, estado, montos
//y la foto del contrato. Si el cliente usara ese mismo, cada campo que la oficina necesita
//obligatorio le rompe el envio, y ademas podria mandar cosas que no le corresponden.
public class MyServiceRequestDto
{
    public Guid ContractClientId { get; set; }

    //Que le esta pasando: lo unico que escribe
    public string ClientReason { get; set; } = null!;

    //A que numero llamarlo, si no es el del contrato
    public string? ContactPhone { get; set; }

    //Si pidio que ese numero quede guardado en su contrato
    public bool UpdateContractPhone { get; set; }
}
