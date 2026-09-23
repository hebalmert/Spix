namespace Spix.Domain.EntitiesContratos;

//La revision de la reactivacion: a quien se le va a devolver el servicio y en que equipo.
//Solo entran los que quedaron suspendidos por el corte y ya pagaron.
public class ActivationCheckDto
{
    //Los que pagaron y esperan que les devuelvan el acceso
    public int Ready { get; set; }

    //De esos, los que no se pueden tocar en el equipo
    public int NoBinding { get; set; }

    public int NoQueue { get; set; }

    //Los que si se pueden reactivar ya
    public int ToActivate { get; set; }

    //La reactivacion se lanza equipo por equipo: una conexion por servidor
    public List<ActivationServerDto> Servers { get; set; } = new();
}

//Un servidor con los contratos que se le van a reactivar
public class ActivationServerDto
{
    //Vacio = contratos sin IpBinding, que no viven en ningun equipo
    public Guid ServerId { get; set; }

    public string? ServerName { get; set; }

    public int Contracts { get; set; }
}

//El resultado de reactivar un equipo. Un contrato con problema no detiene al resto.
public class ActivationRunResultDto
{
    public int Contracts { get; set; }

    public int Activated { get; set; }

    public int Skipped { get; set; }

    public List<ActivationIssueDto> Issues { get; set; } = new();
}

public class ActivationIssueDto
{
    public long ControlContrato { get; set; }

    public string ClientFullName { get; set; } = null!;

    public string Reason { get; set; } = null!;
}

//Un contrato reactivado, como se ve en el listado del modulo
public class ActivationDetailDto
{
    public long ControlContrato { get; set; }

    public string ClientFullName { get; set; } = null!;

    public string? ZoneName { get; set; }

    public string? ServerName { get; set; }

    public DateTime DateSuspended { get; set; }

    public DateTime? DatePaymentReceived { get; set; }

    public bool HasBinding { get; set; }

    public bool HasQueue { get; set; }
}
