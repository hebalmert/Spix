namespace Spix.DomainLogic.EntitiesNetDTO;

//Un cliente con ubicacion, para pintarlo en el mapa de la OLT
public class OltMapClientDto
{
    public Guid ContractClientId { get; set; }

    public long ControlContrato { get; set; }

    public string ClientName { get; set; } = null!;

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    //Distancia en linea recta a la OLT; null si la OLT no tiene coordenadas.
    //Es a vuelo de pajaro, no el recorrido real de la fibra.
    public double? DistanceKm { get; set; }
}
