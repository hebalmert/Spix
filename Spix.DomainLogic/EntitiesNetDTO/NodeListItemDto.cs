namespace Spix.DomainLogic.EntitiesNetDTO;

//Un nodo en el listado: sin usuario ni clave, con sus clientes ya contados por la base
public class NodeListItemDto
{
    public Guid NodeId { get; set; }

    public string NodesName { get; set; } = null!;

    public string? OperationName { get; set; }

    public string? ZoneName { get; set; }

    public string? Ip { get; set; }

    public bool Active { get; set; }

    //Contratos que salen por este nodo
    public int Clients { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }
}
