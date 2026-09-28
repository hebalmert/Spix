using Spix.DomainLogic.EnumTypes;

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

// La credencial PPPoE del contrato. Solo hay una por contrato, garantizado por indice.
public class SuspendPppoeDTO
{
    public Guid ServerId { get; set; }

    public string? ServerName { get; set; }

    public string? ServerIp { get; set; }

    public string? Usuario { get; set; }

    public string? Clave { get; set; }

    public int ApiPort { get; set; }

    // El .id del /ppp/secret: todo lo que Spix creo se toca por su id
    public string? MikrotikId { get; set; }

    // Con el usuario se busca la sesion viva; con la IP se comprueba que sea la suya
    public string? UsuarioPppoe { get; set; }

    public string? IpCliente { get; set; }
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

    // Como trabaja el EQUIPO del contrato. Antes era un bool por corporacion, asi que PPPoE
    // caia en la misma rama que Ninguno y el contrato quedaba Suspendido navegando.
    public MikrotikControlType Control { get; set; } = MikrotikControlType.Ninguno;

    public bool UsaHotSpot => Control == MikrotikControlType.HotSpot;

    public bool UsaControl => Control != MikrotikControlType.Ninguno;

    public SuspendPppoeDTO? Credencial { get; set; }

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

    public MikrotikControlType Control { get; set; } = MikrotikControlType.Ninguno;

    public bool UsaHotSpot => Control == MikrotikControlType.HotSpot;

    public bool UsaControl => Control != MikrotikControlType.Ninguno;

    // En PPPoE el MkIndex guardado es el .id del /ppp/secret. Para tumbar la sesion hace
    // falta ademas el usuario, y la IP para comprobar que la sesion sea la suya.
    public string? UsuarioPppoe { get; set; }

    public string? IpCliente { get; set; }

    public string? Blocked { get; set; }

    public bool CanReactivate => string.IsNullOrWhiteSpace(Blocked);
}
