namespace Spix.Domain.EntitiesPayment;

//Los numeros del tablero de pagos adelantados, contados por la base
public class PrePaymentSummaryDto
{
    //Adelantos recibidos que todavia no se cruzan con una nota de cobro
    public int Pending { get; set; }

    public decimal PendingTotal { get; set; }

    //Contratos distintos con adelanto pendiente
    public int Contracts { get; set; }

    //Lo que ya se cruzo con notas de cobro en el mes en curso
    public int BilledMonth { get; set; }

    public decimal BilledMonthTotal { get; set; }
}
