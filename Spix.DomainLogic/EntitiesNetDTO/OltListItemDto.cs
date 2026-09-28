namespace Spix.DomainLogic.EntitiesNetDTO;

//Una OLT en el listado: sin usuario ni clave, con sus clientes ya contados por la base
public class OltListItemDto
{
    public Guid OltId { get; set; }

    public string OltName { get; set; } = null!;

    public string? MarkName { get; set; }

    public string? ZoneName { get; set; }

    public string? Ip { get; set; }

    //Cuantos puertos PON tiene y a que velocidad va cada uno
    public int PortCount { get; set; }

    public string? PortSpeed { get; set; }

    public bool Active { get; set; }

    //Contratos que entran por esta OLT
    public int Clients { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }
}
