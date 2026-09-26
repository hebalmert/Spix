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

// Todo lo que el ESCRITORIO necesita para cortarle el acceso al lote de UN equipo.
//
// El escritorio habla con el MikroTik por la red LAN, porque el cliente puede no tener IP
// publica. Quien entra en el lote lo decide el servidor: aqui solo viaja el resultado.
public class CorteMkSetupDTO
{
    // Si la corporacion no controla el acceso por HotSpot no se toca el equipo: el lote se
    // suspende y ya
    public bool UsaHotSpot { get; set; }

    // El lote de los que NO tienen IpBinding: no viven en ningun equipo, asi que no hay
    // como quitarles el acceso. Se reportan y no se tocan.
    public bool SinEquipo { get; set; }

    // Al cortar el acceso queda en regular: el cliente cae en el portal del HotSpot
    public string? TipoRegular { get; set; }

    public List<CorteMkContractDTO> Contracts { get; set; } = new();

    public List<CorteMkBindingDTO> Bindings { get; set; } = new();

    public string? Blocked { get; set; }

    public bool CanRun => string.IsNullOrWhiteSpace(Blocked);
}

// Lo que el escritorio devuelve DESPUES de haberle quitado el acceso en el equipo: los
// contratos que el MikroTik SI acepto. Los que fallaron no viajan y no se dan por cortados.
public class CorteMkSaveDTO
{
    public List<Guid> Suspendidos { get; set; } = new();
}
