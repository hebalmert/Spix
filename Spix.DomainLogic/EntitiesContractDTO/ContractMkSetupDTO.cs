namespace Spix.DomainLogic.EntitiesContractDTO;

// Todo lo que el ESCRITORIO necesita para armar por su cuenta la Queue de un contrato.
//
// El escritorio habla con el MikroTik por la red LAN, porque el cliente puede no tener IP
// publica. Para eso tiene que saber lo mismo que sabe el Backend cuando lo hace el: el
// servidor al que conectarse, las velocidades del plan, los Queue Types de la corporacion
// y —esto es lo que no se podia pedir por ningun lado— si el queue padre ya existe y que
// IPs cuelgan de el.
//
// Es de SOLO LECTURA: aqui no se decide nada, se entregan los datos. El calculo y las
// ordenes al equipo los hace el escritorio.
public class ContractQueSetupDTO
{
    //===== El servidor al que hay que conectarse =====
    public Guid ServerId { get; set; }

    public string? ServerName { get; set; }

    public string? ServerIp { get; set; }

    public string? Usuario { get; set; }

    public string? Clave { get; set; }

    public int ApiPort { get; set; }

    //===== El cliente =====
    public Guid IpNetId { get; set; }

    public string? IpCliente { get; set; }

    // "Nombre Apellido - (numero de contrato)", que es como se nombra la queue
    public string? NombreCliente { get; set; }

    //===== El plan =====
    public Guid PlanId { get; set; }

    public string? PlanName { get; set; }

    public string? VelocidadUp { get; set; }

    public string? VelocidadDown { get; set; }

    public string? VelocidadTotal { get; set; }

    // Ya convertidas a kbps por el Backend, que es como las pide el equipo
    public int SpeedUpKbps { get; set; }

    public int SpeedDownKbps { get; set; }

    public int TasaReuso { get; set; }

    //===== Los Queue Types de la corporacion =====
    public string? PcqUp { get; set; }

    public string? PcqDown { get; set; }

    //===== El queue padre =====
    // Si ya existe, el escritorio NO lo crea: solo cuelga el hijo
    public bool HasParent { get; set; }

    public string? ParentMikrotikId { get; set; }

    // Como se llama en el equipo: es lo que se le pone al hijo en "=parent="
    public string? ParentName { get; set; }

    // Las IPs de los clientes que ya tienen queue en ese servidor y ese plan.
    // De aqui salen el target del padre y el calculo de su velocidad.
    public List<string> ClientIps { get; set; } = new();

    //===== Lo que el escritorio no tiene que calcular dos veces =====
    // Motivo por el que NO se puede crear la queue, si lo hay
    public string? Blocked { get; set; }

    public bool CanCreate => string.IsNullOrWhiteSpace(Blocked);
}

// Lo mismo para el IpBinding: con el servidor, la IP y la MAC le alcanza, pero los datos
// de conexion tampoco estaban expuestos en un solo sitio.
public class ContractBindSetupDTO
{
    public Guid ServerId { get; set; }

    public string? ServerName { get; set; }

    public string? ServerIp { get; set; }

    public string? Usuario { get; set; }

    public string? Clave { get; set; }

    public int ApiPort { get; set; }

    public Guid IpNetId { get; set; }

    public string? IpCliente { get; set; }

    public Guid CargueDetailId { get; set; }

    public string? MacCliente { get; set; }

    public string? NombreCliente { get; set; }

    public string? Blocked { get; set; }

    public bool CanCreate => string.IsNullOrWhiteSpace(Blocked);
}

// Lo que el escritorio devuelve DESPUES de haber escrito la Queue en el equipo.
//
// El MikroTik ya quedo configurado por la LAN: esto es solo para que quede el registro en
// la base. Por eso viaja el id que devolvio el equipo, y si hubo que crear el queue padre.
public class ContractQueSaveDTO
{
    public Guid ContractClientId { get; set; }

    public Guid ServerId { get; set; }

    public Guid IpNetId { get; set; }

    public Guid PlanId { get; set; }

    public string? ServerName { get; set; }

    public string? IpServer { get; set; }

    public string? IpCliente { get; set; }

    public string? PlanName { get; set; }

    public string? TotalVelocidad { get; set; }

    // El id que devolvio el equipo para la queue del cliente
    public string? MikrotikId { get; set; }

    //===== El queue padre =====
    // true si el escritorio tuvo que crearlo; false si ya existia y solo se actualiza
    public bool ParentCreated { get; set; }

    public string? ParentName { get; set; }

    public string? ParentMikrotikId { get; set; }

    // Como quedaron las velocidades del padre, en el formato del equipo ("2048k")
    public string? ParentUp { get; set; }

    public string? ParentDown { get; set; }
}

// Lo que el escritorio devuelve despues de haber quitado la Queue del equipo
public class ContractQueRemoveDTO
{
    public Guid ContractQueId { get; set; }

    // true si al quitar este cliente el padre se quedo sin nadie y se borro del equipo
    public bool ParentRemoved { get; set; }

    // Si el padre sigue, como quedaron sus velocidades
    public string? ParentUp { get; set; }

    public string? ParentDown { get; set; }
}

// Lo que el escritorio necesita para QUITAR la Queue del equipo.
//
// Quitar no es solo borrar la queue del cliente: al irse, el queue padre se queda con un
// cliente menos, asi que hay que recalcularle la velocidad y cambiarle el target. Y si se
// queda sin nadie, el padre tambien se borra. Por eso viajan las IPs que QUEDAN.
public class ContractQueRemoveSetupDTO
{
    //===== El servidor al que hay que conectarse =====
    public Guid ServerId { get; set; }

    public string? ServerName { get; set; }

    public string? ServerIp { get; set; }

    public string? Usuario { get; set; }

    public string? Clave { get; set; }

    public int ApiPort { get; set; }

    //===== La queue del cliente que se va =====
    public Guid ContractQueId { get; set; }

    public string? MikrotikId { get; set; }

    //===== El queue padre =====
    public string? ParentMkId { get; set; }

    public string? ParentName { get; set; }

    // Las IPs de los clientes que QUEDAN colgados del padre, sin contar al que se va.
    // Si viene vacia, el padre se queda solo y hay que borrarlo del equipo.
    public List<string> ClientIps { get; set; } = new();

    //===== El plan, para recalcular al padre =====
    public int SpeedUpKbps { get; set; }

    public int SpeedDownKbps { get; set; }

    public int TasaReuso { get; set; }

    public string? Blocked { get; set; }

    public bool CanRemove => string.IsNullOrWhiteSpace(Blocked);
}

// Los datos de conexion del servidor de un contrato, para las ordenes que el escritorio ya
// sabe armar solo: editar y quitar el IpBinding. Ahi no hay nada que calcular, solo hace
// falta con quien hablar y como se llama el cliente, que es el comentario que ve el equipo.
public class ContractMkConnectionDTO
{
    public Guid ServerId { get; set; }

    public string? ServerName { get; set; }

    public string? ServerIp { get; set; }

    public string? Usuario { get; set; }

    public string? Clave { get; set; }

    public int ApiPort { get; set; }

    public string? NombreCliente { get; set; }

    public string? Blocked { get; set; }

    public bool CanConnect => string.IsNullOrWhiteSpace(Blocked);
}
