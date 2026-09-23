namespace Spix.Domain.EntitiesPayment;

//Los numeros del tablero de cuentas por cobrar, contados por la base
public class CxCBillSummaryDto
{
    //Notas vivas con saldo: cuantas, cuanto suman y a cuantos clientes
    public int OpenNotes { get; set; }

    public decimal OpenBalance { get; set; }

    public int Debtors { get; set; }

    //Lo que entro en el mes en curso
    public decimal CollectedMonth { get; set; }
}
