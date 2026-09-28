using Spix.DomainLogic.EnumTypes;

namespace Spix.DomainLogic.EntitiesContractDTO;

// Un contrato del lote del corte. Sirve para reportar a los que quedan fuera con su nombre
// y su numero, sin tener que volver a pedir nada.
public class CorteMkContractDTO
{
    public Guid ContractClientId { get; set; }

    public long ControlContrato { get; set; }

    public string? ClientFullName { get; set; }
}

// Un IpBinding que hay que tocar en el equipo para quitarle el acceso.
//
// Viene con los datos de conexion de SU servidor: un contrato podria tener bindings en
// equipos distintos, y el escritorio abre una conexion por equipo, igual que el Backend.
public class CorteMkBindingDTO
{
    public Guid ContractClientId { get; set; }

    public long ControlContrato { get; set; }

    public string? ClientFullName { get; set; }

    //===== El servidor al que hay que conectarse =====
    public Guid ServerId { get; set; }

    public string? ServerName { get; set; }

    public string? ServerIp { get; set; }

    public string? Usuario { get; set; }

    public string? Clave { get; set; }

    public int ApiPort { get; set; }

    //===== Lo que se manda en el set =====
    public string? MikrotikId { get; set; }

    public string? IpCliente { get; set; }

    public string? MacCliente { get; set; }

    // "Nombre Apellido - (numero de contrato)", ya armado por el servidor
    public string? Comentario { get; set; }
}

// Una credencial PPPoE que hay que deshabilitar en el equipo para quitarle el acceso.
//
// El gemelo de CorteMkBindingDTO, con los datos de conexion de SU servidor. En vez de la MAC
// lleva el usuario, que es lo que identifica la sesion viva en /ppp/active.
public class CortePppoeDTO
{
    public Guid ContractClientId { get; set; }

    public long ControlContrato { get; set; }

    public string? ClientFullName { get; set; }

    //===== El servidor al que hay que conectarse =====
    public Guid ServerId { get; set; }

    public string? ServerName { get; set; }

    public string? ServerIp { get; set; }

    public string? Usuario { get; set; }

    public string? Clave { get; set; }

    public int ApiPort { get; set; }

    //===== Lo que se manda al equipo =====

    // El .id del /ppp/secret. Todo lo que Spix creo se toca por su id, nunca por nombre.
    public string? MikrotikId { get; set; }

    // El usuario PPPoE: con el se busca la sesion viva que hay que tumbar
    public string? UsuarioPppoe { get; set; }

    // La IP fija del contrato: se cruza con la de la sesion antes de tumbarla
    public string? IpCliente { get; set; }
}

// Todo lo que el ESCRITORIO necesita para cortarle el acceso al lote de UN equipo.
//
// El escritorio habla con el MikroTik por la red LAN, porque el cliente puede no tener IP
// publica. Quien entra en el lote lo decide el servidor: aqui solo viaja el resultado.
public class CorteMkSetupDTO
{
    // Como trabaja el EQUIPO de este corte. Antes era un bool por corporacion, asi que PPPoE
    // caia en la misma rama que Ninguno y el lote se suspendia sin tocar el equipo.
    public MikrotikControlType Control { get; set; } = MikrotikControlType.Ninguno;

    // Se conserva calculado para que nada que todavia lo lea cambie de comportamiento
    public bool UsaHotSpot => Control == MikrotikControlType.HotSpot;

    public bool UsaControl => Control != MikrotikControlType.Ninguno;

    // El lote de los que NO tienen IpBinding: no viven en ningun equipo, asi que no hay
    // como quitarles el acceso. Se reportan y no se tocan.
    public bool SinEquipo { get; set; }

    // Al cortar el acceso queda en regular: el cliente cae en el portal del HotSpot
    public string? TipoRegular { get; set; }

    public List<CorteMkContractDTO> Contracts { get; set; } = new();

    public List<CorteMkBindingDTO> Bindings { get; set; } = new();

    public List<CortePppoeDTO> Credenciales { get; set; } = new();

    public string? Blocked { get; set; }

    public bool CanRun => string.IsNullOrWhiteSpace(Blocked);
}

// Lo que el escritorio devuelve DESPUES de haberle quitado el acceso en el equipo: los
// contratos que el MikroTik SI acepto. Los que fallaron no viajan y no se dan por cortados.
public class CorteMkSaveDTO
{
    public List<Guid> Suspendidos { get; set; } = new();
}
