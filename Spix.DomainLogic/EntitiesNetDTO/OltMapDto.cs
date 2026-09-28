using Spix.DomainLogic.ModelUtility;

namespace Spix.DomainLogic.EntitiesNetDTO;

//Todo lo que pinta el Mapa de OLT para UNA OLT, en un solo request
public class OltMapDto
{
    public Guid OltId { get; set; }

    public string OltName { get; set; } = null!;

    public string? Ip { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    //El tablero
    public int Clients { get; set; }

    public int WithLocation { get; set; }

    public int WithoutLocation { get; set; }

    public double? FarthestKm { get; set; }

    //Lo propio de la fibra: cuantos puertos PON tiene el equipo
    public int PortCount { get; set; }

    public string? PortSpeed { get; set; }

    //Los puntos del mapa: clientes con ubicacion, del mas cercano al mas lejano
    public List<OltMapClientDto> Located { get; set; } = new();

    //Los clientes que solo se sabe que salen por la OLT: combo ya armado con su neutro traducido
    public List<GuidNameModel> UnlocatedOptions { get; set; } = new();
}
