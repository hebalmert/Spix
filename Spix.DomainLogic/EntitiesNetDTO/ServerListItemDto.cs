using Spix.DomainLogic.EnumTypes;

namespace Spix.DomainLogic.EntitiesNetDTO;

//Un servidor en el listado: sin usuario ni clave, con sus clientes ya contados por la base
public class ServerListItemDto
{
    public Guid ServerId { get; set; }

    public string ServerName { get; set; } = null!;

    public string? ZoneName { get; set; }

    public string? Ip { get; set; }

    public bool Active { get; set; }

    //Contratos que salen por este servidor
    public int Clients { get; set; }

    //Como trabaja el equipo, para el pill del listado
    public MikrotikControlType ControlMk { get; set; }

    //Si el equipo PPPoE ya tiene su servidor PPPoE creado
    public bool PppoeReady { get; set; }
}
