namespace Spix.Domain.EntitiesBilling;

//Los numeros del tablero de notas de cobro generales, contados por la base
public class BillingNoteSummaryDto
{
    public int YearNumber { get; set; }

    //Notas del ano en curso
    public int Notes { get; set; }

    public int Launched { get; set; }

    public int Pending { get; set; }

    //Contratos activos hoy: los que entrarian en el proximo lanzamiento
    public int ActiveContracts { get; set; }

    //Lo facturado en el ano por notas individuales
    public decimal Billed { get; set; }
}
