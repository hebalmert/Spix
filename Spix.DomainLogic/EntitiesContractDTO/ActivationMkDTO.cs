namespace Spix.DomainLogic.EntitiesContractDTO;

// Un contrato del lote que hay que tocar en el equipo para devolverle el acceso.
//
// Viene con TODO lo que va en la orden, ya armado por el servidor —incluido el comentario,
// que se compone igual que en el Backend— para que el escritorio no tenga que calcular
// nada y el registro del equipo quede identico se reactive desde donde se reactive.
public class ActivationMkBindingDTO
{
    public Guid ContractClientId { get; set; }

    public long ControlContrato { get; set; }

    public string? ClientFullName { get; set; }

    //Lo que se manda en el set. Puede venir vacio: al contrato le falta configuracion y el
    //escritorio lo cuenta como que quedo fuera, sin tocar el equipo.
    public string? MikrotikId { get; set; }

    public string? IpCliente { get; set; }

    public string? MacCliente { get; set; }

    // "Nombre Apellido - (numero de contrato)", igual que lo arma el Backend
    public string? Comentario { get; set; }
}

// Todo lo que el ESCRITORIO necesita para reactivar el lote de UN servidor.
//
// El escritorio habla con el MikroTik por la red LAN, porque el cliente puede no tener IP
// publica. El servidor entrega los datos de conexion y la lista; las reglas de quien entra
// en el lote se quedan de este lado.
public class ActivationMkSetupDTO
{
    //===== El servidor al que hay que conectarse =====
    public Guid ServerId { get; set; }

    public string? ServerName { get; set; }

    public string? ServerIp { get; set; }

    public string? Usuario { get; set; }

    public string? Clave { get; set; }

    public int ApiPort { get; set; }

    // El tipo que deja pasar el trafico: es lo contrario de lo que pone el corte
    public string? TipoBypassed { get; set; }

    // Si la corporacion no controla el acceso por HotSpot no se toca el equipo:
    // el lote se da por bueno y solo cambia el estado
    public bool UsaHotSpot { get; set; }

    public List<ActivationMkBindingDTO> Bindings { get; set; } = new();

    public string? Blocked { get; set; }

    public bool CanActivate => string.IsNullOrWhiteSpace(Blocked);
}

// Lo que el escritorio devuelve DESPUES de haber escrito el equipo: los contratos que el
// MikroTik SI acepto. Los que fallaron no viajan, para que no se den por reactivados.
public class ActivationMkSaveDTO
{
    public List<Guid> Activados { get; set; } = new();
}
