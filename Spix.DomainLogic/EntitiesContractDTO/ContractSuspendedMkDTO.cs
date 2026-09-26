namespace Spix.DomainLogic.EntitiesContractDTO;

// Un IpBinding que hay que tocar en el equipo para suspender. Trae con el los datos de
// conexion del servidor, porque un contrato puede tener bindings en servidores distintos
// y el escritorio abre una conexion por servidor, no una por binding.
public class SuspendBindingDTO
{
    public Guid ServerId { get; set; }

    public string? ServerName { get; set; }

    public string? ServerIp { get; set; }

    public string? Usuario { get; set; }

    public string? Clave { get; set; }

    public int ApiPort { get; set; }

    //Lo que se manda en el set
    public string? MikrotikId { get; set; }

    public string? IpCliente { get; set; }

    public string? MacCliente { get; set; }
}

// Todo lo que el ESCRITORIO necesita para suspender un contrato en el equipo.
//
// El escritorio habla con el MikroTik por la red LAN, porque el cliente puede no tener IP
// publica. Las validaciones —que el contrato este activo, que el binding este en bypassed—
// se quedan en el servidor: si algo falla viene Blocked y aqui no hay nada que escribir.
public class SuspendMkSetupDTO
{
    // El comentario con el que queda el registro en el equipo
    public string? NombreCliente { get; set; }

    // Al suspender el acceso queda en regular: el cliente cae en el portal del HotSpot
    public string? TipoRegular { get; set; }

    public List<SuspendBindingDTO> Bindings { get; set; } = new();

    // Si la corporacion no usa HotSpot no se toca el equipo: solo cambia el estado
    public bool UsaHotSpot { get; set; }

    public string? Blocked { get; set; }

    public bool CanSuspend => string.IsNullOrWhiteSpace(Blocked);
}

// Lo mismo para devolver el acceso. Aqui basta un set con el id y el tipo, y el id sale
// del que quedo guardado en la suspension, no del IpBinding de hoy.
public class ReactivateMkSetupDTO
{
    public Guid ServerId { get; set; }

    public string? ServerName { get; set; }

    public string? ServerIp { get; set; }

    public string? Usuario { get; set; }

    public string? Clave { get; set; }

    public int ApiPort { get; set; }

    public string? MkIndex { get; set; }

    // Al reactivar vuelve a bypassed: el cliente pasa sin portal
    public string? TipoBypassed { get; set; }

    public bool UsaHotSpot { get; set; }

    public string? Blocked { get; set; }

    public bool CanReactivate => string.IsNullOrWhiteSpace(Blocked);
}
