namespace Spix.DomainLogic.EntitiesNetDTO;

//Un cliente con ubicacion, para pintarlo en el mapa del nodo
public class NodeMapClientDto
{
    public Guid ContractClientId { get; set; }

    public long ControlContrato { get; set; }

    public string ClientName { get; set; } = null!;

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    //Distancia en linea recta al nodo; null si el nodo no tiene coordenadas
    public double? DistanceKm { get; set; }

    //Rumbo desde el nodo hacia el cliente, en grados (0 = norte, 90 = este).
    //Con el se sabe si el cliente cae dentro de la mascara de cobertura.
    public double? BearingDeg { get; set; }
}
