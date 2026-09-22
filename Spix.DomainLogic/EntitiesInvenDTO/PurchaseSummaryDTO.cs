namespace Spix.DomainLogic.EntitiesInvenDTO;

//Los numeros del tablero de compras, contados por la base
public class PurchaseSummaryDto
{
    //Compras abiertas: todavia no movieron inventario
    public int OpenPurchases { get; set; }

    //Compras cerradas en el mes en curso
    public int MonthPurchases { get; set; }

    //Lo comprado en el mes en curso, con impuesto
    public decimal MonthTotal { get; set; }
}
