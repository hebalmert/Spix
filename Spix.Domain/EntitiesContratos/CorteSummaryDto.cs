namespace Spix.Domain.EntitiesContratos;

//Los numeros del tablero de cortes, contados por la base
public class CorteSummaryDto
{
    //Contratos con la suspension abierta hoy
    public int Suspended { get; set; }

    //Contratos que deben algo, y cuanto suman
    public int Debtors { get; set; }

    public decimal DebtTotal { get; set; }

    //Cortes creados que todavia no se han ejecutado
    public int Pending { get; set; }
}
