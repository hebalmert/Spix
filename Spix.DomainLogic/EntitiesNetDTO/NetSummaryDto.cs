namespace Spix.DomainLogic.EntitiesNetDTO;

//Los numeros del tablero de Nodos o de Servidores, contados por la base
public class NetSummaryDto
{
    public int Total { get; set; }

    public int Active { get; set; }

    public int Inactive { get; set; }

    //Contratos que salen por los equipos de este listado
    public int Clients { get; set; }
}
