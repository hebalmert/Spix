namespace Spix.DomainLogic.EntitiesNetDTO;

//Los numeros del tablero de un listado de IP (clientes o red), contados por la base
public class IpSummaryDto
{
    public int Total { get; set; }

    //Activas, sin asignar y sin excluir: las que se pueden usar ya
    public int Free { get; set; }

    public int Assigned { get; set; }

    public int Excluded { get; set; }
}
