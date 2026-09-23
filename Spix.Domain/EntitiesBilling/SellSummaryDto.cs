namespace Spix.Domain.EntitiesBilling;

//Los numeros del tablero de facturas, contados por la base y solo del mes en curso
public class SellSummaryDto
{
    //Facturas emitidas este mes, sin contar las anuladas
    public int MonthCount { get; set; }

    public decimal MonthTotal { get; set; }

    //De esas, cuantas ya estan pagadas
    public int MonthPaid { get; set; }

    //Las que se anularon en el mes
    public int MonthCancelled { get; set; }
}
