namespace Spix.DomainLogic.EntitiesNetDTO;

//Una OLT en la vista de todas: solo lo que necesita su punto en el mapa
public class OltMapItemDto
{
    public Guid OltId { get; set; }

    public string OltName { get; set; } = null!;

    public string? Ip { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    //Contratos que entran por esta OLT
    public int Clients { get; set; }
}
