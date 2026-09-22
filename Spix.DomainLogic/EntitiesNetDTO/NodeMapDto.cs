using Spix.DomainLogic.ModelUtility;

namespace Spix.DomainLogic.EntitiesNetDTO;

//Todo lo que pinta el Mapa de nodos para UN nodo, en un solo request
public class NodeMapDto
{
    public Guid NodeId { get; set; }

    public string NodesName { get; set; } = null!;

    public string? Ip { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    //El tablero
    public int Clients { get; set; }

    public int WithLocation { get; set; }

    public int WithoutLocation { get; set; }

    public double? FarthestKm { get; set; }

    //Los puntos del mapa: clientes con ubicacion, del mas cercano al mas lejano
    public List<NodeMapClientDto> Located { get; set; } = new();

    //Los clientes que solo se sabe que salen por el AP: combo ya armado con su neutro traducido
    public List<GuidNameModel> UnlocatedOptions { get; set; } = new();
}
