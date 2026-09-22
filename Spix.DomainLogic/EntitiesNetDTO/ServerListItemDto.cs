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
}
